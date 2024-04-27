using Microsoft.AspNetCore.Mvc;
using StartaneousAPI.Models;
using StarTaneousAPI.Models;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StartaneousAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private static List<GameMatch> Games = new List<GameMatch>();
        private static bool CreatingMatch = false;
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger)
        {
            _logger = logger;
        }

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
                    Thread.Sleep(500);
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
            int playerPosition = Array.FindIndex(game.Players, x => x.StationId == completedTurn.ClientId);
            //Generate Module
            if (completedTurn.Actions != null && completedTurn.Actions.Any(x => x.actionTypeId == (int)ActionType.GenerateModule))
            {
                Random rnd = new Random();
                completedTurn.Actions.Where(x => x.actionTypeId == (int)ActionType.GenerateModule).ToList().ForEach(x => x = GenerateModel(x, rnd));
            }            
            GameTurn? gameTurn = game.GameTurns?.FirstOrDefault(x => x.TurnNumber == completedTurn.TurnNumber);
            if (gameTurn == null)
            {
                game.GameTurns.Add(new GameTurn(completedTurn.TurnNumber, completedTurn, playerPosition));
            }
            else
            {
                gameTurn.ClientActions[playerPosition] = completedTurn;
            }
            return true;
        }

        private ActionIds GenerateModel(ActionIds x, Random rnd)
        {
            x.generatedModuleId = rnd.Next(0, 11);
            x.generatedGuid = Guid.NewGuid();
            return x;
        }

        [HttpGet]
        [Route("Join")]
        public Guid Join(Guid ClientId)
        {
            while (CreatingMatch)
            {
                Thread.Sleep(500);
            }
            CreatingMatch = true;
            //Todo search for opening, to scale to 4 players
            GameMatch? matchToJoin = Games.FirstOrDefault(x => x.Players[1] == null);
            if (matchToJoin != null)
            {
                matchToJoin.Players[1] = new Player(ClientId);
            }
            else
            {
                matchToJoin = new GameMatch();
                matchToJoin.Players[0] = new Player(ClientId);
                Games.Add(matchToJoin);
            }
            CreatingMatch = false;
            return matchToJoin.GameId;
        }
        
        [HttpGet]
        [Route("HasGameStarted")]
        public Player[]? HasGameStarted(Guid GameId)
        {
            Player[]? clients = null;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (clients == null && timer.Elapsed.TotalSeconds < 10)
            {
                var game = Games.FirstOrDefault(x => x.GameId == GameId);
                if (game != null && game.Players.All(x => x != null))
                {
                    clients = game.Players;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
            timer.Stop();
            return clients;
        }
    }
}