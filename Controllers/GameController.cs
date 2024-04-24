using Microsoft.AspNetCore.Mvc;
using StartaneousAPI.Models;
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
        private int maxPlayers = 2;
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
                if(turns != null && turns.All(x => x?.Actions != null)) {
                    turnsToReturn = turns;
                }
                System.Threading.Thread.Sleep(500);
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
            int playerPosition = Array.FindIndex(game.Clients, x => x == completedTurn.ClientId);
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


        [HttpGet]
        [Route("Join")]
        public Guid Join(Guid ClientId)
        {
            while (CreatingMatch)
            {
                System.Threading.Thread.Sleep(500);
            }
            CreatingMatch = true;
            GameMatch? matchToJoin = Games.FirstOrDefault(x => x.Clients[1] == Guid.Empty);
            if (matchToJoin != null)
            {
                matchToJoin.Clients[1] = ClientId;
            }
            else
            {
                matchToJoin = new GameMatch();
                matchToJoin.Clients[0] = ClientId;
                Games.Add(matchToJoin);
            }
            CreatingMatch = false;

            return matchToJoin.GameId;

        }
    }
}