using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SidusAPI.Data;
using SidusAPI.ServerModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SidusAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        [HttpGet]
        [Route("GetTurns")]
        public GameTurn? GetTurns(Guid gameGuid, int turnNumber, bool quickSearch)
        {
            using (var context = new ApplicationDbContext())
            {
                try
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    do
                    {
                        var gameTurn = context.GameMatches
                            .Include(gm => gm.GameTurns)
                                .ThenInclude(gt => gt.Players)
                                    .ThenInclude(p => p.Units)
                            .Include(gm => gm.GameTurns)
                                .ThenInclude(gt => gt.Players)
                                    .ThenInclude(p => p.Actions)
                            .Include(gm => gm.GameTurns)
                                .ThenInclude(gt => gt.Players)
                                    .ThenInclude(p => p.Technology)
                            .Include(gm => gm.GameTurns)
                                .ThenInclude(gt => gt.MarketModuleGuids)
                            .Include(gm => gm.GameTurns)
                                .ThenInclude(gt => gt.AllNodes)
                            .FirstOrDefault(x => x.GameGuid == gameGuid)?.GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
                        if (gameTurn != null && gameTurn.TurnIsOver)
                        {
                            return gameTurn;
                        }
                        else if (!quickSearch)
                        {
                            Thread.Sleep(250);
                        }
                    } while (timer.Elapsed.TotalSeconds < 10 && !quickSearch);
                    timer.Stop();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                    return null;
                }
                return null;
            }
        }

        [HttpPost]
        [Route("EndTurn")]
        public bool EndTurn([FromBody] GameTurn currentTurn)
        {
            using (var context = new ApplicationDbContext())
            {
                try
                {
                    GameMatch? serverGame = context.GameMatches
                        .Include(gm => gm.GameTurns)
                            .ThenInclude(gt => gt.Players)
                                .ThenInclude(p => p.Units)
                        .Include(gm => gm.GameTurns)
                            .ThenInclude(gt => gt.Players)
                                .ThenInclude(p => p.Actions)
                        .Include(gm => gm.GameTurns)
                            .ThenInclude(gt => gt.Players)
                                .ThenInclude(p => p.Technology)
                        .Include(gm => gm.GameTurns)
                            .ThenInclude(gt => gt.MarketModuleGuids)
                        .Include(gm => gm.GameTurns)
                            .ThenInclude(gt => gt.AllNodes)
                        .FirstOrDefault(x => x.GameGuid == currentTurn.GameGuid);
                    GamePlayer? playerTurn = currentTurn.Players?.FirstOrDefault();
                    if (serverGame == null || playerTurn == null)
                    {
                        return false;
                    }
                    GameTurn? gameTurn = serverGame.GameTurns?.FirstOrDefault(x => x.TurnNumber == currentTurn.TurnNumber);
                    if (gameTurn == null)
                    {
                        currentTurn.TurnIsOver = false;
                        serverGame.GameTurns.Add(currentTurn);
                    }
                    else if (!gameTurn.TurnIsOver)
                    {
                        GamePlayer? submittedTurn = gameTurn.Players?.FirstOrDefault(x => x.PlayerId == playerTurn.PlayerId);
                        if (submittedTurn == null)
                        {
                            gameTurn.Players.Add(playerTurn);
                        }
                        else
                        {
                            gameTurn.Players[submittedTurn.PlayerId] = playerTurn;
                        }
                    }
                    gameTurn = serverGame.GameTurns.FirstOrDefault(x => x.TurnNumber == currentTurn.TurnNumber);
                    //Do market stuff if everyone is done with their turns
                    if (gameTurn.Players?.Count() == serverGame.MaxPlayers && !gameTurn.TurnIsOver)
                    {
                        //Decrease turn timer and reset old modules
                        foreach (var moduleGuid in gameTurn.MarketModuleGuids.Split(","))
                        {
                            var module = gameTurn.AllModules.FirstOrDefault(x => x.ModuleGuid == Guid.Parse(moduleGuid));
                            if (module.TurnsLeft > 1)
                            {
                                module.TurnNumber++;
                                module.TurnsLeft--;
                                module.MidBid = (module.TurnsLeft * 2) + 1;
                            }
                            else
                            {
                                var newModule = GetNewServerModule(GetIntListFromString(gameTurn.ModulesForMarket), serverGame.NumberOfModules, (serverGame.MaxPlayers == 1 ? 4 : serverGame.MaxPlayers), gameTurn.GameGuid, gameTurn.TurnNumber+1);
                                gameTurn.MarketModuleGuids.Replace(moduleGuid, newModule.ModuleGuid.ToString());
                                module.ModuleGuid = newModule.ModuleGuid;
                                module.ModuleId = newModule.ModuleId;
                                module.MidBid = newModule.MidBid;
                                module.TurnsLeft = newModule.TurnsLeft;
                                module.TurnNumber = newModule.TurnNumber;
                                module.GameGuid = newModule.GameGuid;
                            }
                        }
                        //Check on bid wars
                        var bidGroup = gameTurn.Players?.SelectMany(x => x?.Actions)?.Where(y => y.ActionTypeId == (int)ActionType.BidOnModule).GroupBy(x => x.SelectedModuleGuid);
                        foreach (var bid in bidGroup)
                        {
                            var module = gameTurn.AllModules.FirstOrDefault(x => x.ModuleGuid == bid.Key);
                            if (bid.Count() > 1)
                            {
                                var bidsInOrder = bid.OrderByDescending(x => x.PlayerBid).ThenBy(x => x.ActionOrder).ToList();
                                bidsInOrder[0].PlayerBid = bidsInOrder[1].PlayerBid;
                                for (var i = 1; i < bidsInOrder.Count(); i++)
                                {
                                    bidsInOrder[i].SelectedModuleGuid = null;
                                }
                            }
                            else
                            {
                                bid.FirstOrDefault().PlayerBid = module.MidBid;
                            }
                            //could have already been rotated out
                            if (module != null)
                            {
                                var newModule = GetNewServerModule(GetIntListFromString(gameTurn.ModulesForMarket), serverGame.NumberOfModules, (serverGame.MaxPlayers == 1 ? 4 : serverGame.MaxPlayers), gameTurn.GameGuid, gameTurn.TurnNumber);
                                gameTurn.MarketModuleGuids.Replace(module.ModuleGuid.ToString(), newModule.ModuleGuid.ToString());
                                module.ModuleGuid = newModule.ModuleGuid;
                                module.ModuleId = newModule.ModuleId;
                                module.MidBid = newModule.MidBid;
                                module.TurnsLeft = newModule.TurnsLeft;
                                module.TurnNumber = newModule.TurnNumber;
                                module.GameGuid = newModule.GameGuid;
                            }
                        }
                        gameTurn.TurnIsOver = true;
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                    return false;
                }
                return true;
            }
        }

        private ServerModule GetNewServerModule(List<int> numMods, int maxNum, int maxTurns, Guid gameGuid, int turnNumber)
        {
            if (numMods.Count <= 0)
            {
                for (int i = 0; i <= maxNum; i++)
                {
                    numMods.Add(i);
                }
            }
            Random rnd = new Random();
            int createIndex = rnd.Next(0, numMods.Count);
            var moduleToCreate = numMods[createIndex];
            numMods.RemoveAt(createIndex);
            return new ServerModule()
            {
                GameGuid = gameGuid,
                TurnNumber = turnNumber,
                ModuleGuid = Guid.NewGuid(),
                ModuleId = moduleToCreate,
                TurnsLeft = maxTurns,
                MidBid = (maxTurns * 2) + 1,
            };
        }

        [HttpGet]
        [Route("FindGames")]
        public List<GameMatch> FindGames(Guid? playerGuid = null)
        {
            using (var context = new ApplicationDbContext())
            {
                var serverGames = context.GameMatches
                    .Include(gm => gm.GameTurns)
                    .ThenInclude(gt => gt.Players).Where(x => !x.IsDeleted && x.GameTurns.Count > 0);
                var gamesToRemove = serverGames.Where(x => x.GameTurns[0].Players.Count() < x.MaxPlayers && x.HealthCheck < DateTime.Now.AddHours(-24)).ToList();
                foreach(var game in gamesToRemove)
                {
                    game.IsDeleted = true;
                }
                context.SaveChanges();
                if (playerGuid != null)
                    return serverGames.Where(x => x.Winner == -1 && x.GameTurns[0].Players.Count() > 1 && (x.GameTurns[0].Players.Any(x => x.PlayerGuid == playerGuid))).ToList();
                return serverGames.Where(x => x.GameTurns[0].Players.Count() < x.MaxPlayers).ToList();
            }
        } 
        
        [HttpPost]
        [Route("JoinGame")]
        public GameMatch? JoinGame(GameMatch ClientGame)
        {
            using (var context = new ApplicationDbContext())
            {
                GameMatch? matchToJoin = context.GameMatches.AsQueryable()
                    .Include(gm => gm.GameTurns)
                    .ThenInclude(gt => gt.Players)
                        .ThenInclude(p => p.Units)
                    .Include(gm => gm.GameTurns)
                        .ThenInclude(gt => gt.Players)
                            .ThenInclude(p => p.Actions)
                    .Include(gm => gm.GameTurns)
                        .ThenInclude(gt => gt.Players)
                            .ThenInclude(p => p.Technology)
                    .Include(gm => gm.GameTurns)
                        .ThenInclude(gt => gt.MarketModuleGuids)
                    .Include(gm => gm.GameTurns)
                    .ThenInclude(gt => gt.AllNodes)
                    .FirstOrDefault(x => x.GameGuid == ClientGame.GameGuid);
                if (matchToJoin != null && matchToJoin.GameTurns?[0]?.Players != null)
                {
                    var currentPlayerCount = matchToJoin.GameTurns[0].Players.Count();
                    if (currentPlayerCount < matchToJoin.MaxPlayers)
                    {
                        if (ClientGame.GameTurns[0].Players.Count() > currentPlayerCount)
                        {
                            var newPlayer = ClientGame.GameTurns[0].Players.Last();
                            matchToJoin.GameTurns[0].Players.Add(newPlayer);
                            context.SaveChanges();
                            return matchToJoin;
                        }
                    }
                    else
                    {
                        return matchToJoin;
                    }
                }
                return null;
            }
        }
        
        [HttpPost]
        [Route("CreateGame")]
        public GameMatch CreateGame([FromBody] GameMatch ClientGame)
        {
            ClientGame.GameGuid = ClientGame.GameGuid;
            Random rnd = new Random();
            List<Guid> newModules = new List<Guid>();
            for (int i = 0; i < (ClientGame.MaxPlayers == 1 ? 4 : ClientGame.MaxPlayers); i++)
            {
                var newModule = GetNewServerModule(GetIntListFromString(ClientGame.GameTurns[0].ModulesForMarket), ClientGame.NumberOfModules, (ClientGame.MaxPlayers == 1 ? 4 : ClientGame.MaxPlayers), ClientGame.GameGuid, 0);
                ClientGame.GameTurns[0].AllModules.Add(newModule);
                newModules.Add(newModule.ModuleGuid);
            }
            ClientGame.GameTurns[0].MarketModuleGuids = String.Join(",", newModules);
            ClientGame.Winner = -1;
            ClientGame.HealthCheck = DateTime.Now;
            using (var context = new ApplicationDbContext())
            {
                context.GameMatches.Add(ClientGame);
                context.SaveChanges();
            }
            return ClientGame;
        }
        public List<int> GetIntListFromString(string? csvString)
        {
            if(String.IsNullOrEmpty(csvString))
                return new List<int>();
            return csvString.Split(",").Select(x => int.Parse(x)).ToList();
        }
        [HttpGet]
        [Route("EndGame")]
        public int EndGame(Guid gameGuid, int winner)
        {
            using (var context = new ApplicationDbContext())
            {
                var serverGame = context.GameMatches.FirstOrDefault(x => x.GameGuid == gameGuid);
                if (winner != -1)
                    serverGame.Winner = winner;
                context.SaveChanges();
                return serverGame.Winner;
            }
        }

        [HttpGet]
        [Route("HasTakenTurn")]
        public GameTurn? HasTakenTurn(Guid gameGuid, int turnNumber)
        {
            using (var context = new ApplicationDbContext())
            {
                var serverGame = context.GameMatches
                    .Include(gm => gm.GameTurns)
                    .ThenInclude(gt => gt.Players)
                    .FirstOrDefault(x => x.GameGuid == gameGuid);
                if (serverGame != null)
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    var gameTurn = serverGame?.GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
                    var startPlayers = gameTurn?.Players?.Count();
                    while (timer.Elapsed.TotalSeconds < 10)
                    {
                        serverGame.HealthCheck = DateTime.Now;
                        context.SaveChanges();
                        gameTurn = context.GameMatches
                            .Include(gm => gm.GameTurns)
                            .ThenInclude(gt => gt.Players)
                            .FirstOrDefault(x => x.GameGuid == gameGuid).GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
                        var currentPlayers = gameTurn?.Players?.Count();
                        if (startPlayers != currentPlayers || (currentPlayers == serverGame.MaxPlayers))
                        {
                            return gameTurn;
                        }
                        else
                        {
                            Thread.Sleep(500);
                        }
                    }
                    timer.Stop();
                    return gameTurn;
                }
                return null;
            }
        }
    }
}