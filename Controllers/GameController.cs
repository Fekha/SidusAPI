using Microsoft.AspNetCore.Mvc;
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
        private static bool SubmittingTurn = false;

        [HttpGet]
        [Route("GetTurns")]
        public GameTurn? GetTurns(Guid gameGuid, int turnNumber, bool quickSearch)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            do {
                var gameTurn = ServerGames.FirstOrDefault(x => x.GameGuid == gameGuid)?.GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
                if (!SubmittingTurn && gameTurn != null && AllSubmittedTurn(gameTurn))
                {
                    return gameTurn;
                }
                else if(!quickSearch)
                {
                    Thread.Sleep(250);
                }
            } while (timer.Elapsed.TotalSeconds < 10 && !quickSearch);
            timer.Stop();
            return null;
        }

        [HttpPost]
        [Route("EndTurn")]
        public bool EndTurn([FromBody] GameTurn currentTurn)
        {
            while (SubmittingTurn)
            {
                Thread.Sleep(250);
            }
            SubmittingTurn = true;
            GameMatch? serverGame = ServerGames.FirstOrDefault(x => x.GameGuid == currentTurn.GameGuid);
            int playerIndex = -1;
            Player? playerTurn = null;
            for (int i = 0; i < currentTurn.Players?.Count(); i++)
            {
                if (currentTurn.Players[i] != null)
                {
                    playerIndex = i;
                    playerTurn = currentTurn.Players[i];
                }
            }
            if (serverGame == null || playerTurn == null || playerIndex == -1)
            {
                SubmittingTurn = false;
                return false;
            }
            GameTurn? gameTurn = serverGame.GameTurns?.FirstOrDefault(x => x.TurnNumber == currentTurn.TurnNumber);
            if (gameTurn == null)
            {
                serverGame.GameTurns.Add(currentTurn);
                gameTurn = currentTurn;
            }
            else
            {
                gameTurn.Players[playerIndex] = playerTurn;
            }
            //Do market stuff if everyone is done with their turns
            if (AllSubmittedTurn(gameTurn))
            {
                //Decrease turn timer and reset old modules
                foreach (var module in gameTurn.MarketModules)
                {
                    if (module.TurnsLeft > 1)
                    {
                        module.MidBid--;
                        module.TurnsLeft--;
                    }
                    else
                    {
                        var newModule = GetNewServerModule(serverGame.NumberOfModules);
                        module.ModuleGuid = newModule.ModuleGuid;
                        module.ModuleId = newModule.ModuleId;
                        module.MidBid = newModule.MidBid;
                        module.TurnsLeft = newModule.TurnsLeft;
                    }
                    module.PlayerBid = module.MidBid;
                }
                //Check on bid wars
                var bidGroup = gameTurn.Players?.SelectMany(x => x?.Actions)?.Where(y => y.ActionTypeId == (int)ActionType.BidOnModule).GroupBy(x => x.SelectedModule.ModuleGuid);
                foreach (var bid in bidGroup)
                {
                    if (bid.Count() > 1)
                    {
                        var bidsInOrder = bid.OrderByDescending(x => x.SelectedModule.PlayerBid).ThenBy(x => x.ActionOrder).ToList();
                        bidsInOrder[0].SelectedModule.PlayerBid = bidsInOrder[1].SelectedModule.PlayerBid;
                        for (var i = 1; i < bidsInOrder.Count(); i++)
                        {
                            bidsInOrder[i].SelectedModule = null;
                        }
                    }
                    //reset bought module
                    var module = gameTurn.MarketModules?.FirstOrDefault(x => x.ModuleGuid == bid.Key);
                    var newModule = GetNewServerModule(serverGame.NumberOfModules);
                    module.ModuleGuid = newModule.ModuleGuid;
                    module.ModuleId = newModule.ModuleId;
                    module.MidBid = newModule.MidBid;
                    module.TurnsLeft = newModule.TurnsLeft;
                }
            }
            SubmittingTurn = false;
            return true;
        }

        private ServerModule GetNewServerModule(int numMods)
        {
            Random rnd = new Random();
            return new ServerModule()
            {
                ModuleGuid = Guid.NewGuid(),
                ModuleId = rnd.Next(0, numMods),
                MidBid = 6,
                TurnsLeft = 5,
            };
        }

        private bool AllSubmittedTurn(GameTurn? gameTurn)
        {
            return gameTurn?.Players?.All(x => x != null) ?? false;
        }

        [HttpGet]
        [Route("FindGames")]
        public List<GameMatch> FindGames()
        {
            return ServerGames.Where(x => (x.GameTurns[0]?.Players?.Any(y => y == null) ?? false)).ToList();
        } 
        
        [HttpPost]
        [Route("JoinGame")]
        public GameMatch? JoinGame(GameMatch ClientGame)
        {
            GameMatch? matchToJoin = ServerGames.FirstOrDefault(x => x.GameGuid == ClientGame.GameGuid && (x.GameTurns[0]?.Players?.Any(y=> y == null) ?? false));
            if (matchToJoin != null)
            {
                for (var i = (matchToJoin.GameTurns[0]?.Players?.Count()-1 ?? 0); i >= 0 ; i--) {
                    if (matchToJoin.GameTurns[0]?.Players[i] == null)
                    {
                        matchToJoin.GameTurns[0].Players[i] = ClientGame.GameTurns[0].Players[1];
                        return matchToJoin;
                    }
                }
            }
            return null;
        }
        
        [HttpPost]
        [Route("CreateGame")]
        public GameMatch CreateGame([FromBody] GameMatch ClientGame)
        {
            ClientGame.GameGuid = Guid.NewGuid();
            Random rnd = new Random();
            for (int i = 0; i < 2 + ClientGame.MaxPlayers; i++)
            {
                ClientGame.GameTurns[0].MarketModules.Add(GetNewServerModule(ClientGame.NumberOfModules));
            }
            ServerGames.Add(ClientGame);
            return ClientGame;
        }
        
        [HttpGet]
        [Route("HasGameStarted")]
        public GameMatch? HasGameStarted(Guid GameGuid)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            var game = ServerGames.FirstOrDefault(x => x.GameGuid == GameGuid);
            if (game != null)
            {
                var startPlayers = game.GameTurns[0]?.Players?.Count(x => x != null);
                while (timer.Elapsed.TotalSeconds < 10)
                {
                    var currentPlayers = game.GameTurns[0]?.Players?.Count(x => x != null);
                    if (startPlayers != currentPlayers || (game.GameTurns[0]?.Players?.All(x => x != null) ?? false))
                    {
                        return game;
                    }
                    else
                    {
                        Thread.Sleep(250);
                    }
                }
            }
            timer.Stop();
            return game;
        }
    }
}