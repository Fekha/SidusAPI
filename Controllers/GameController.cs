using Microsoft.AspNetCore.Mvc;
using StartaneousAPI.ServerModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StartaneousAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private static List<GameMatch> ServerGames = new List<GameMatch>();

        [HttpGet]
        [Route("GetTurns")]
        public GameTurn? GetTurns(Guid gameGuid, int turnNumber)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (timer.Elapsed.TotalSeconds < 10)
            {
                var turns = ServerGames.FirstOrDefault(x => x.GameGuid == gameGuid)?.GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber && (x.Players?.All(x => x != null) ?? false));
                if (turns != null)
                {
                    return turns;
                }
                else
                {
                    Thread.Sleep(250);
                }
            }
            timer.Stop();
            return null;
        }

        [HttpPost]
        [Route("EndTurn")]
        public bool EndTurn([FromBody] GameTurn currentTurn)
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
            //Generate Server side action values
            if (playerTurn.Actions != null) {
                if (playerTurn.Actions.Any(x => x.ActionTypeId == (int)ActionType.GenerateModule))
                {
                    Random rnd = new Random();
                    playerTurn.Actions.Where(x => x.ActionTypeId == (int)ActionType.GenerateModule).ToList().ForEach(x => x = GenerateModel(x, rnd));
                }
            }
            GameTurn? gameTurn = serverGame.GameTurns?.FirstOrDefault(x => x.TurnNumber == currentTurn.TurnNumber);
            if (gameTurn == null)
            {
                serverGame.GameTurns.Add(currentTurn);
            }
            else
            {
                gameTurn.Players[playerIndex] = playerTurn;
            }
            return true;
        }

        private ServerModels.ServerAction GenerateModel(ServerModels.ServerAction x, Random rnd)
        {
            x.GeneratedModuleId = rnd.Next(0, 23);
            x.GeneratedGuid = Guid.NewGuid();
            return x;
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
            return null;
        }
    }
}