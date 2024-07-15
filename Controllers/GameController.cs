using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SidusAPI.Data;
using SidusAPI.Enums;
using SidusAPI.ServerModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
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
        public ActionResult<GameTurn?> GetTurns(Guid gameGuid, int turnNumber, int searchType, int startPlayers, int clientVersion)
        {
            try
            {
                var clientVersionText = CheckClientVersion(clientVersion);
                if (!String.IsNullOrEmpty(clientVersionText)) { return BadRequest(clientVersionText); }
                var serverGame = GetServerMatch(gameGuid);
                if (serverGame == null) { return NotFound(); }
                serverGame.HealthCheck = DateTime.Now;
                if (serverGame.MaxPlayers > 1) //Dont save practice games
                    UpdateDBHealthCheck(gameGuid, serverGame.HealthCheck);
                GameTurn? gameTurn;
                Stopwatch timer = new Stopwatch();
                timer.Start();
                do
                {
                    gameTurn = GetServerMatch(gameGuid).GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
                    if (gameTurn != null)
                    {
                        int currentPlayers = gameTurn?.Players?.Count() ?? 0;
                        if (searchType == (int)SearchType.gameSearch && (startPlayers != currentPlayers || gameTurn.TurnIsOver))
                        {
                            return gameTurn;
                        }
                        else if (searchType == (int)SearchType.lobbySearch && (startPlayers != currentPlayers || currentPlayers == serverGame.MaxPlayers))
                        {
                            return gameTurn;
                        }
                    }
                    Thread.Sleep(250);
                } while (timer.Elapsed.TotalSeconds < 10);
                timer.Stop();
                return gameTurn;
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex}");
            }
        }

        [HttpPost]
        [Route("EndTurn")]
        public ActionResult<bool> EndTurn([FromBody] GameTurn currentTurn, int clientVersion)
        {
            try
            {
                var clientVersionText = CheckClientVersion(clientVersion);
                if (!String.IsNullOrEmpty(clientVersionText)) { return BadRequest(clientVersionText); }
                GameMatch? serverGame = GetServerMatch(currentTurn.GameGuid);
                GamePlayer? playerTurn = currentTurn.Players?.FirstOrDefault();
                if (serverGame == null || playerTurn == null) { return NotFound(); }
                GameTurn? gameTurn = serverGame.GameTurns?.FirstOrDefault(x => x.TurnNumber == currentTurn.TurnNumber);
                if (gameTurn == null)
                {
                    currentTurn.TurnIsOver = false;
                    serverGame.GameTurns.Add(currentTurn);
                    gameTurn = currentTurn;
                    if (serverGame.MaxPlayers > 1) //Dont save practice games
                        UpdateDBCreateTurn(gameTurn);
                }
                else if (!gameTurn.TurnIsOver)
                {
                    int turnIndex = gameTurn.Players.Select(x=>x.PlayerGuid).ToList().IndexOf(playerTurn.PlayerGuid);
                    if (turnIndex == -1)
                    {
                        gameTurn.Players.Add(playerTurn);
                    }
                    else
                    {
                        gameTurn.Players[turnIndex] = playerTurn;
                    }
                    if (serverGame.MaxPlayers > 1) //Dont save practice games
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
                                module.MidBid = (2 * module.TurnsLeft) + 4;
                            }
                            else
                            {
                                var newModule = GetNewServerModule(gameTurn, serverGame.NumberOfModules, 3, gameTurn.GameGuid, gameTurn.TurnNumber);
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
                            bidsInOrder.FirstOrDefault().PlayerBid = bidsInOrder[1].PlayerBid;
                            for (var i = 1; i < bidsInOrder.Count(); i++)
                            {
                                bidsInOrder[i].SelectedModuleGuid = null;
                            }
                        }
                        else
                        {
                            bid.FirstOrDefault().PlayerBid = module.MidBid;
                        }
                        var newModule = GetNewServerModule(gameTurn, serverGame.NumberOfModules, 3, gameTurn.GameGuid, gameTurn.TurnNumber);
                        gameTurn.MarketModuleGuids = gameTurn.MarketModuleGuids.Replace(module.ModuleGuid.ToString(), newModule.ModuleGuid.ToString());
                        gameTurn.AllModules.Add(newModule);
                    }
                    gameTurn.TurnIsOver = true;
                    foreach (var player in gameTurn.Players)
                    {
                        if (player.PlayerGuid != playerTurn.PlayerGuid)
                        {
                            NotifyPlayerTurnAsync(player.PlayerGuid, gameTurn.GameGuid);
                        }
                    }
                    if (serverGame.MaxPlayers > 1) //Dont save practice games
                        UpdateDBCreateTurn(gameTurn);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex}");
            }
            return true;
        }

        [HttpGet]
        [Route("FindGames")]
        public ActionResult<List<GameMatch>> FindGames(int clientVersion, Guid? playerGuid = null)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var clientVersionText = CheckClientVersion(clientVersion);
                    if (!String.IsNullOrEmpty(clientVersionText)) { return BadRequest(clientVersionText); }
                    var serverGames = context.GameMatches.Include(gm => gm.GameTurns)
                    .ThenInclude(gt => gt.Players).Where(x => !x.IsDeleted && x.GameTurns.Count > 0).ToList(); 
                    var gamesToRemove = serverGames.Where(x => x.GameTurns.FirstOrDefault().Players.Count() < x.MaxPlayers && x.HealthCheck < DateTime.Now.AddHours(-24)).ToList();
                    foreach (var game in gamesToRemove)
                    {
                        game.IsDeleted = true;
                    }
                    UpdateDBSave(context);
                    if (playerGuid != null)
                        serverGames = serverGames.Where(x => !x.IsDeleted && x.MaxPlayers>1 && x.Winner == Guid.Empty && x.GameTurns.FirstOrDefault().Players.Any(x => x.PlayerGuid == playerGuid)).ToList();
                    else
                        serverGames = serverGames.Where(x => !x.IsDeleted && x.GameTurns.FirstOrDefault().Players.Count() < x.MaxPlayers).ToList();
                    return serverGames;
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex}");
            }
        }

        [HttpPost]
        [Route("JoinGame")]
        public ActionResult<GameMatch?> JoinGame(GameMatch ClientGame, int clientVersion)
        {
            try
            {
                var clientVersionText = CheckClientVersion(clientVersion);
                if (!String.IsNullOrEmpty(clientVersionText)) { return BadRequest(clientVersionText); }
                GameMatch? matchToJoin = GetServerMatch(ClientGame.GameGuid);
                if (matchToJoin == null || matchToJoin.GameTurns?.FirstOrDefault()?.Players == null) { return NotFound(); }
                var currentPlayerCount = matchToJoin.GameTurns.FirstOrDefault().Players.Count();
                if (currentPlayerCount < matchToJoin.MaxPlayers && ClientGame.GameTurns.FirstOrDefault().Players.Count() > currentPlayerCount)
                {
                    var newPlayer = ClientGame.GameTurns.FirstOrDefault().Players.OrderBy(x => x.PlayerColor).Last();
                    matchToJoin.GameTurns.FirstOrDefault().Players.Add(newPlayer);
                    UpdateDBAddPlayer(newPlayer);
                    return matchToJoin;
                }
                else
                {
                    return matchToJoin;
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex}");
            }
        }

        [HttpPost]
        [Route("CreateGame")]
        public ActionResult<GameMatch?> CreateGame([FromBody] GameMatch ClientGame, int clientVersion)
        {
            try
            {
                var clientVersionText = CheckClientVersion(clientVersion);
                if (!String.IsNullOrEmpty(clientVersionText)) { return BadRequest(clientVersionText); }
                ClientGame.GameGuid = ClientGame.GameGuid;
                Random rnd = new Random();
                List<Guid> newModules = new List<Guid>();
                for (int i = 0; i < 3; i++)
                {
                    var newModule = GetNewServerModule(ClientGame.GameTurns.FirstOrDefault(), ClientGame.NumberOfModules, i + 1, ClientGame.GameGuid, 0);
                    ClientGame.GameTurns.FirstOrDefault().AllModules.Add(newModule);
                    newModules.Add(newModule.ModuleGuid);
                }
                ClientGame.GameTurns.FirstOrDefault().MarketModuleGuids = String.Join(",", newModules);
                ClientGame.Winner = Guid.Empty;
                ClientGame.HealthCheck = DateTime.Now;
                if (ClientGame.MaxPlayers > 1) //Dont save practice games
                    UpdateDBCreateGame(ClientGame);
                ServerGames.Add(ClientGame);
                return ClientGame;
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex}");
            }
        }

        [HttpGet]
        [Route("EndGame")]
        public ActionResult<Guid> EndGame(Guid gameGuid, Guid winner, int clientVersion)
        {
            try
            {
                var clientVersionText = CheckClientVersion(clientVersion);
                if (!String.IsNullOrEmpty(clientVersionText)) { return BadRequest(clientVersionText); }
                using (var context = new ApplicationDbContext())
                {
                    var serverGame = GetServerMatch(gameGuid);
                    var game = context.GameMatches.FirstOrDefault(x => x.GameGuid == gameGuid);
                    if (game != null && winner != Guid.Empty && serverGame.Winner == Guid.Empty && game.Winner == Guid.Empty)
                    {
                        serverGame.Winner = winner;
                        game.Winner = winner;
                        context.Accounts.FirstOrDefault(x => x.PlayerGuid == winner).Wins++;
                        if (serverGame.MaxPlayers == 2 && serverGame.GameTurns.Count > 5)
                            UpdateRatings(winner, serverGame.GameTurns[0].Players.FirstOrDefault(x => x.PlayerGuid != winner).PlayerGuid);
                        context.SaveChanges();
                    }
                    return serverGame.Winner;
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex}");
            }
        }

        private GameMatch? GetServerMatch(Guid gameGuid)
        {
            var game = ServerGames.FirstOrDefault(x => x.GameGuid == gameGuid);
            if (game == null)
            {
                using (var context = new ApplicationDbContext())
                {
                    game = context.GameMatches.FirstOrDefault(x => x.GameGuid == gameGuid);
                    game.GameTurns = context.GameTurns.Where(x => x.GameGuid == gameGuid).OrderBy(x => x.TurnNumber).ToList();
                    foreach (var gameTurn in game.GameTurns)
                    {
                        gameTurn.Players = context.GamePlayers.Where(x => x.GameGuid == gameGuid && x.TurnNumber == gameTurn.TurnNumber).ToList();
                        gameTurn.AllModules = context.ServerModules.Where(x => x.GameGuid == gameGuid && x.TurnNumber == gameTurn.TurnNumber).ToList();
                        gameTurn.AllNodes = context.ServerNodes.Where(x => x.GameGuid == gameGuid && x.TurnNumber == gameTurn.TurnNumber).ToList();
                        foreach (var player in gameTurn.Players)
                        {
                            player.Actions = context.ServerActions.Where(x => x.GameGuid == gameGuid && x.TurnNumber == gameTurn.TurnNumber && x.PlayerGuid == player.PlayerGuid).ToList();
                            player.Technology = context.ServerTechnologies.Where(x => x.GameGuid == gameGuid && x.TurnNumber == gameTurn.TurnNumber && x.PlayerGuid == player.PlayerGuid).ToList();
                            player.Units = context.ServerUnits.Where(x => x.GameGuid == gameGuid && x.TurnNumber == gameTurn.TurnNumber && x.PlayerGuid == player.PlayerGuid).ToList();
                        }
                    }
                    if (game != null)
                    {
                        ServerGames.Add(game);
                    }
                }
            }
            return game;
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
                MidBid = (2 * maxTurns) + 4,
            };
        }
        public List<int> GetIntListFromString(string? csvString)
        {
            if (String.IsNullOrEmpty(csvString))
                return new List<int>();
            return csvString.Split(",").Select(x => int.Parse(x)).ToList();
        }
        private string CheckClientVersion(int clientVersion)
        {
            using (var context = new ApplicationDbContext())
            {
                var serverVersion = context.Settings.FirstOrDefault().ClientVersion;
                return serverVersion > clientVersion ? $"New Client version {serverVersion} available. Current Client version {clientVersion}" : "";
            }
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
                        turn.ModulesForMarket = gameTurn.ModulesForMarket;
                        turn.TurnIsOver = gameTurn.TurnIsOver;
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
            try
            {
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
                        context.ServerActions.RemoveRange(context.ServerActions.Where(x => x.GameGuid == newPlayer.GameGuid && x.TurnNumber == newPlayer.TurnNumber && x.PlayerGuid == newPlayer.PlayerGuid));
                        context.ServerActions.AddRange(newPlayer.Actions);
                    }
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }

        public async Task NotifyPlayerTurnAsync(Guid playerGuid, Guid gameGuid)
        {
            using (var context = new ApplicationDbContext())
            {
                try
                {
                    var player = context.Accounts.FirstOrDefault(x => x.PlayerGuid == playerGuid);
                    if (player != null && player.NotifiyByEmail == true && !String.IsNullOrEmpty(player.Email))
                    {
                        var subject = "Your Turn in Sydus!";
                        var body = $"Log in to make a move in your game {gameGuid}.";
                        await EmailService.SendEmailAsync(player.Email, subject, body);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }

        public double CalculateExpectedScore(double playerRating, double opponentRating)
        {
            return 1.0 / (1.0 + Math.Pow(10, (opponentRating - playerRating) / 400));
        }

        public void UpdateRatings(Guid winnerGuid, Guid loserGuid, int k = 32)
        {
            using (var context = new ApplicationDbContext())
            {
                try
                {
                    var winner = context.Accounts.FirstOrDefault(x => x.PlayerGuid == winnerGuid);
                    var loser = context.Accounts.FirstOrDefault(x => x.PlayerGuid == loserGuid);
                    double expectedWinner = CalculateExpectedScore((double)winner.Rating, (double)loser.Rating);
                    double expectedLoser = CalculateExpectedScore((double)loser.Rating, (double)winner.Rating);
                    winner.Rating = winner.Rating + k * (1 - expectedWinner);
                    loser.Rating = loser.Rating + k * (0 - expectedLoser);
                    context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }
    }
}