using Microsoft.AspNetCore.Mvc;
using StartaneousAPI.Models;
using StarTaneousAPI.Models;
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
        private static List<GameMatch> Games = new List<GameMatch>();

        [HttpGet]
        [Route("GetTurn")]
        public Turn[]? GetTurn(Guid gameId, int turnNumber)
        {
            Turn[]? turnsToReturn = null;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (turnsToReturn == null && timer.Elapsed.TotalSeconds < 10)
            {
                var turns = Games.FirstOrDefault(x => x.GameId == gameId)?.GameTurns?.FirstOrDefault(x => x.TurnNumber == turnNumber)?.ClientActions;
                if (turns != null && turns.All(x => x?.Actions != null))
                {
                    turnsToReturn = turns;
                }
                else
                {
                    Thread.Sleep(250);
                }
            }
            timer.Stop();
            return turnsToReturn;
        }

        [HttpPost]
        [Route("EndTurn")]
        public bool EndTurn([FromBody] Turn completedTurn)
        {
            GameMatch? game = Games.FirstOrDefault(x => x.GameId == completedTurn.GameId);
            if (game == null)
            {
                return false;
            }
            int playerPosition = Array.FindIndex(game.Players, x => x.StationGuid == completedTurn.ClientId);
            //Generate Server side action values
            if (completedTurn.Actions != null) {
                if (completedTurn.Actions.Any(x => x.actionTypeId == (int)ActionType.GenerateModule))
                {
                    Random rnd = new Random();
                    completedTurn.Actions.Where(x => x.actionTypeId == (int)ActionType.GenerateModule).ToList().ForEach(x => x = GenerateModel(x, rnd));
                }
                if (completedTurn.Actions.Any(x => x.actionTypeId == (int)ActionType.CreateFleet))
                {
                    completedTurn.Actions.Where(x => x.actionTypeId == (int)ActionType.CreateFleet).ToList().ForEach(x => x.generatedGuid = Guid.NewGuid());
                }
            }
            GameTurn? gameTurn = game.GameTurns?.FirstOrDefault(x => x.TurnNumber == completedTurn.TurnNumber);
            if (gameTurn == null)
            {
                game.GameTurns.Add(new GameTurn(completedTurn.TurnNumber, completedTurn, playerPosition, game.MaxPlayers));
            }
            else
            {
                gameTurn.ClientActions[playerPosition] = completedTurn;
            }
            return true;
        }

        private ActionIds GenerateModel(ActionIds x, Random rnd)
        {
            x.generatedModuleId = rnd.Next(0, 23);
            x.generatedGuid = Guid.NewGuid();
            return x;
        }

        [HttpGet]
        [Route("Find")]
        public List<NewGame> Find()
        {
            var openGames = Games.Where(x => x.Players.Any(y => y == null)).ToList();
            List<NewGame> games = new List<NewGame>();
            foreach (var game in openGames)
            {
                NewGame ClientGame = new NewGame();
                ClientGame.GameId = game.GameId;
                ClientGame.MaxPlayers = game.MaxPlayers;
                ClientGame.PlayerCount = game.Players.Where(y => y != null).Count();
                games.Add(ClientGame);
            }
            return games;
        } 
        
        [HttpGet]
        [Route("Join")]
        public NewGame? Join(Guid ClientId, Guid GameId)
        {
            GameMatch? matchToJoin = Games.FirstOrDefault(x => x.GameId == GameId && x.Players.Any(y=> y == null));
            if (matchToJoin != null)
            {
                for (var i = 0; i < matchToJoin.Players.Count(); i++) {
                    if (matchToJoin.Players[i] == null)
                    {
                        matchToJoin.Players[i] = new Player(ClientId);
                        var ClientGame = new NewGame();
                        ClientGame.ClientId = ClientId;
                        ClientGame.GameId = matchToJoin.GameId;
                        ClientGame.MaxPlayers = matchToJoin.MaxPlayers;
                        ClientGame.PlayerCount = matchToJoin.Players.Where(y => y != null).Count();
                        ClientGame.GameSettings = matchToJoin.GameSettings;
                        return ClientGame;
                    }
                }
            }
            return null;
        }
        
        [HttpPost]
        [Route("Create")]
        public NewGame Create([FromBody] NewGame ClientGame)
        {
            GameMatch newMatch = new GameMatch(ClientGame);
            Games.Add(newMatch);
            ClientGame.GameId = newMatch.GameId;
            ClientGame.PlayerCount = 1;
            return ClientGame;
        }
        
        [HttpGet]
        [Route("HasGameStarted")]
        public Player[]? HasGameStarted(Guid GameId)
        {
            Player[]? clients = null;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            var game = Games.FirstOrDefault(x => x.GameId == GameId);
            if (game != null)
            {
                var startPlayers = game.Players.Count(x => x != null);
                while (clients == null && timer.Elapsed.TotalSeconds < 10)
                {
                    var currentPlayers = game.Players.Count(x => x != null);
                    if (startPlayers != currentPlayers || game.Players.All(x => x != null))
                    {
                        clients = game.Players.Where(x => x != null).ToArray();
                    }
                    else
                    {
                        Thread.Sleep(250);
                    }
                }
            }
            timer.Stop();
            return clients;
        }
    }
}