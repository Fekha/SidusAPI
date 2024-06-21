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
        private static List<GameMatch> ServerGames = new List<GameMatch>();

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
                        var gameTurn = GetServerMatch(gameGuid).GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
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

        private GameMatch GetServerMatch(Guid gameGuid)
        {
            var game = ServerGames.FirstOrDefault(x => x.GameGuid == gameGuid);
            if(game == null)
            {
                using (var context = new ApplicationDbContext())
                {
                    game = context.GameMatches
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
                            .ThenInclude(gt => gt.AllNodes)
                        .Include(gm => gm.GameTurns)
                            .ThenInclude(gt => gt.AllModules)
                        .FirstOrDefault(x => x.GameGuid == gameGuid);
                    if (game != null)
                    {
                        ServerGames.Add(game);
                    }
                }
            }
            return game;
        }

        [HttpPost]
        [Route("EndTurn")]
        public bool EndTurn([FromBody] GameTurn currentTurn)
        {
            try
            {
                GameMatch? serverGame = GetServerMatch(currentTurn.GameGuid);
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
                    gameTurn = currentTurn;
                    UpdateDBCreateTurn(gameTurn);
                }
                else if (!gameTurn.TurnIsOver)
                {
                    GamePlayer? submittedTurn = gameTurn.Players?.FirstOrDefault(x => x.PlayerGuid == playerTurn.PlayerGuid);
                    if (submittedTurn == null)
                    {
                        gameTurn.Players.Add(playerTurn);
                    }
                    else
                    {
                        submittedTurn = playerTurn;
                    }
                    UpdateDBAddPlayer(playerTurn);
                }
                //Do market stuff if everyone is done with their turns
                if (gameTurn.Players?.Count() == serverGame.MaxPlayers && !gameTurn.TurnIsOver)
                {
                    var bidGroup = gameTurn.Players?.SelectMany(x => x?.Actions)?.Where(y => y.ActionTypeId == (int)ActionType.BidOnModule).GroupBy(x => x.SelectedModuleGuid);
                    //Decrease turn timer and reset old modules
                    foreach (var moduleGuid in gameTurn.MarketModuleGuids.Split(",").Select(x => Guid.Parse(x)))
                    {
                        if (!bidGroup.Any(x => x.Key == moduleGuid))
                        {
                            var module = gameTurn.AllModules.FirstOrDefault(x => x.ModuleGuid == moduleGuid);
                            if (module.TurnsLeft > 1)
                            {
                                module.TurnsLeft--;
                                module.MidBid = (module.TurnsLeft * 2) + 1;
                            }
                            else
                            {
                                var newModule = GetNewServerModule(gameTurn, serverGame.NumberOfModules, (serverGame.MaxPlayers == 1 ? 4 : serverGame.MaxPlayers), gameTurn.GameGuid, gameTurn.TurnNumber);
                                gameTurn.MarketModuleGuids = gameTurn.MarketModuleGuids.Replace(moduleGuid.ToString(), newModule.ModuleGuid.ToString());
                                gameTurn.AllModules.Add(newModule);
                                gameTurn.AllModules.Remove(module);
                            }
                        }
                    }
                    //Check on bid wars
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
                        var newModule = GetNewServerModule(gameTurn, serverGame.NumberOfModules, (serverGame.MaxPlayers == 1 ? 4 : serverGame.MaxPlayers), gameTurn.GameGuid, gameTurn.TurnNumber);
                        gameTurn.MarketModuleGuids = gameTurn.MarketModuleGuids.Replace(module.ModuleGuid.ToString(), newModule.ModuleGuid.ToString());
                        gameTurn.AllModules.Add(newModule);
                    }
                    gameTurn.TurnIsOver = true;
                    UpdateDBCreateTurn(gameTurn);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }
            return true;
        }

        private ServerModule GetNewServerModule(GameTurn GameTurn, int maxNum, int maxTurns, Guid gameGuid, int turnNumber)
        {
            List<int> numMods = GetIntListFromString(GameTurn.ModulesForMarket);
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
            GameTurn.ModulesForMarket = String.Join(",", numMods);
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
                var serverGames = context.GameMatches.Include(gm => gm.GameTurns)
                    .ThenInclude(gt => gt.Players).Where(x => !x.IsDeleted && x.GameTurns.Count > 0).ToList();
                var gamesToRemove = serverGames.Where(x => x.GameTurns[0].Players.Count() < x.MaxPlayers && x.HealthCheck < DateTime.Now.AddHours(-24)).ToList();
                foreach(var game in gamesToRemove)
                {
                    game.IsDeleted = true;
                }
                UpdateDBSave(context);
                if (playerGuid != null)
                    return serverGames.Where(x => !x.IsDeleted && x.Winner == Guid.Empty && x.GameTurns[0].Players.Any(x => x.PlayerGuid == playerGuid)).ToList();
                return serverGames.Where(x => !x.IsDeleted && x.GameTurns[0].Players.Count() < x.MaxPlayers).ToList();
            }
        }

        [HttpPost]
        [Route("JoinGame")]
        public GameMatch? JoinGame(GameMatch ClientGame)
        {
            GameMatch? matchToJoin = GetServerMatch(ClientGame.GameGuid);
            if (matchToJoin != null && matchToJoin.GameTurns?[0]?.Players != null)
            {
                var currentPlayerCount = matchToJoin.GameTurns[0].Players.Count();
                if (currentPlayerCount < matchToJoin.MaxPlayers && ClientGame.GameTurns[0].Players.Count() > currentPlayerCount)
                {
                    var newPlayer = ClientGame.GameTurns[0].Players.OrderBy(x=>x.PlayerColor).Last();
                    newPlayer.PlayerColor = ClientGame.GameTurns[0].Players.Count()-1;
                    matchToJoin.GameTurns[0].Players.Add(newPlayer);
                    UpdateDBAddPlayer(newPlayer);
                    return matchToJoin;
                }
                else
                {
                    return matchToJoin;
                }
            }
            return null;
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
                var newModule = GetNewServerModule(ClientGame.GameTurns[0], ClientGame.NumberOfModules, (ClientGame.MaxPlayers == 1 ? 4 : ClientGame.MaxPlayers), ClientGame.GameGuid, 0);
                ClientGame.GameTurns[0].AllModules.Add(newModule);
                newModules.Add(newModule.ModuleGuid);
            }
            ClientGame.GameTurns[0].MarketModuleGuids = String.Join(",", newModules);
            ClientGame.Winner = Guid.Empty;
            ClientGame.HealthCheck = DateTime.Now;
            UpdateDBCreateGame(ClientGame);
            ServerGames.Add(ClientGame);
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
        public Guid EndGame(Guid gameGuid, Guid winner)
        {
            using (var context = new ApplicationDbContext())
            {
                var serverGame = GetServerMatch(gameGuid);
                if (winner != Guid.Empty)
                {
                    serverGame.Winner = winner;
                    context.GameMatches.FirstOrDefault(x => x.GameGuid == gameGuid).Winner = winner;
                    context.SaveChanges();
                }
                return serverGame.Winner;
            }
        }

        [HttpGet]
        [Route("HasTakenTurn")]
        public GameTurn? HasTakenTurn(Guid gameGuid, int turnNumber)
        {
            var serverGame = GetServerMatch(gameGuid);
            if (serverGame != null)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                var gameTurn = serverGame?.GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
                var startPlayers = gameTurn?.Players?.Count();
                while (timer.Elapsed.TotalSeconds < 10)
                {
                    serverGame.HealthCheck = DateTime.Now;
                    UpdateDBHealthCheck(gameGuid, serverGame.HealthCheck);
                    gameTurn = GetServerMatch(gameGuid).GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
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

        private async Task UpdateDBHealthCheck(Guid gameGuid, DateTime healthCheck)
        {
            using (var context = new ApplicationDbContext())
            {
                try
                {
                    context.GameMatches.FirstOrDefault(x => x.GameGuid == gameGuid).HealthCheck = healthCheck;
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }
        private async Task UpdateDBCreateGame(GameMatch newGame)
        {
            using (var context = new ApplicationDbContext())
            {
                try
                {
                    context.GameMatches.Add(newGame);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }

        private async Task UpdateDBCreateTurn(GameTurn gameTurn)
        {
            using (var context = new ApplicationDbContext())
            {
                try {
                    var turn = context.GameTurns.FirstOrDefault(x => x.GameGuid == gameTurn.GameGuid && x.TurnNumber == gameTurn.TurnNumber);
                    if (turn == null)
                    {
                        context.GameTurns.Add(gameTurn);
                    }
                    else
                    {
                        turn = gameTurn;
                    }
                    await context.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }

        private async Task UpdateDBSave(ApplicationDbContext context)
        {
            try{
                await context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Debug.Print(ex.ToString());
            }
        }

        private async Task UpdateDBAddPlayer(GamePlayer newPlayer)
        {
            using (var context = new ApplicationDbContext())
            {
                try
                {
                    var gamePlayer = context.GamePlayers.FirstOrDefault(x => x.GameGuid == newPlayer.GameGuid && x.TurnNumber == newPlayer.TurnNumber && x.PlayerGuid == newPlayer.PlayerGuid);
                    if (gamePlayer == null)
                    {
                        context.GamePlayers.Add(newPlayer);
                    }
                    else
                    {
                        gamePlayer = newPlayer;
                    }
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }
    }
}