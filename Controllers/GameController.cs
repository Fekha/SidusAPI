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

        [HttpGet]
        [Route("GetTurns")]
        public GameTurn? GetTurns(Guid gameGuid, int turnNumber, bool quickSearch)
        {
            try
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                do
                {
                    var gameTurn = ServerGames.FirstOrDefault(x => x.GameGuid == gameGuid)?.GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
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

        [HttpPost]
        [Route("EndTurn")]
        public bool EndTurn([FromBody] GameTurn currentTurn)
        {
            try
            {
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
                    return false;
                }
                GameTurn? gameTurn = serverGame.GameTurns?.FirstOrDefault(x => x.TurnNumber == currentTurn.TurnNumber);
                if (gameTurn == null)
                {
                    currentTurn.TurnIsOver = false;
                    serverGame.GameTurns.Add(currentTurn);
                    gameTurn = currentTurn;
                }
                else if (!gameTurn.TurnIsOver)
                {
                    gameTurn.Players[playerIndex] = playerTurn;
                }
                //Do market stuff if everyone is done with their turns
                if (AllSubmittedTurn(gameTurn) && !gameTurn.TurnIsOver)
                {
                    //Decrease turn timer and reset old modules
                    foreach (var module in gameTurn.MarketModules)
                    {
                        if (module.TurnsLeft > 1)
                        {
                            module.MidBid-=2;
                            module.TurnsLeft--;
                        }
                        else
                        {
                            var newModule = GetNewServerModule(gameTurn.ModulesForMarket, serverGame.NumberOfModules);
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
                        else
                        {
                            bid.FirstOrDefault().SelectedModule.PlayerBid = bid.FirstOrDefault().SelectedModule.MidBid;
                        }
                        //reset bought module
                        var module = gameTurn.MarketModules?.FirstOrDefault(x => x.ModuleGuid == bid.Key);
                        //could have already been rotated out
                        if (module != null)
                        {
                            var newModule = GetNewServerModule(gameTurn.ModulesForMarket, serverGame.NumberOfModules);
                            module.ModuleGuid = newModule.ModuleGuid;
                            module.ModuleId = newModule.ModuleId;
                            module.MidBid = newModule.MidBid;
                            module.TurnsLeft = newModule.TurnsLeft;
                        }
                    }
                    gameTurn.TurnIsOver = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }
            return true;
        }

        private ServerModule GetNewServerModule(List<int> numMods, int maxNum)
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
                ModuleGuid = Guid.NewGuid(),
                ModuleId = moduleToCreate,
                MidBid = 5,
                TurnsLeft = 2,
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
            for (int i = ServerGames.Count-1; i >= 0; i--)
            {
                if ((ServerGames[i].GameTurns[0]?.Players?.Any(y => y == null) ?? false) && ServerGames[i].HealthCheck < DateTime.Now.AddSeconds(-15))
                {
                    ServerGames.RemoveAt(i);
                }
            }
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
            for (int i = 0; i < (ClientGame.MaxPlayers == 1 ? 2 : ClientGame.MaxPlayers); i++)
            {
                ClientGame.GameTurns[0].MarketModules.Add(GetNewServerModule(ClientGame.GameTurns[0].ModulesForMarket, ClientGame.NumberOfModules));
            }
            ClientGame.HealthCheck = DateTime.Now;
            ServerGames.Add(ClientGame);
            return ClientGame;
        }
        
        [HttpGet]
        [Route("HasTakenTurn")]
        public GameTurn? HasTakenTurn(Guid gameGuid, int turnNumber)
        {
            var game = ServerGames.FirstOrDefault(x => x.GameGuid == gameGuid);
            if (game != null)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                var gameTurn = game?.GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
                var startPlayers = gameTurn?.Players?.Count(x => x != null) ?? 0;
                while (timer.Elapsed.TotalSeconds < 10)
                {
                    game.HealthCheck = DateTime.Now;
                    gameTurn = game?.GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber);
                    var currentPlayers = gameTurn?.Players?.Count(x => x != null) ?? 0;
                    if (startPlayers != currentPlayers || (gameTurn?.Players?.All(x => x != null) ?? false))
                    {
                        return gameTurn;
                    }
                    else
                    {
                        Thread.Sleep(250);
                    }
                }
                timer.Stop();
                return gameTurn;
            }
            return null;
        }
    }
}