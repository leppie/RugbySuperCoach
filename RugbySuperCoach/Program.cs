using System;
using System.Collections.Generic;
using System.Linq;
using RugbySuperCoachDB;
using Newtonsoft.Json;
using System.IO;

namespace RugbySuperCoach
{
  class Program
  {
    static void Main(string[] args)
    {
      LoadPlayers();
      // run when games are completed
      //LoadRoundFixtures(1);
      // run when games are completed
      //LoadGameData(1);
      // run when points are finalized
      //UpdateGameData(1);

    }

    static void UpdateGameData(int round)
    {
      using (var dc = new RugbySuperCoachDataContext())
      {
        using (var reader = new JsonTextReader(File.OpenText("round" + round + "-players.json")))
        {
          dynamic players = new JsonSerializer().Deserialize(reader);

          var roundplayers = dc.PlayerStats.Where(x => x.Game.Round == round).ToDictionary(x => x.PlayerId);

          foreach (var player in players)
          {
            var pid = (int)player.id;
            PlayerStat stat;

            if (roundplayers.TryGetValue(pid, out stat))
            {
              UpdateStats(player, stat);
            }
          }
        }
        dc.SubmitChanges();
      }
    }

    static void UpdateStats(dynamic player, PlayerStat stats)
    {
      stats.Value = -(int)player.p;
      stats.Total = -(int)player.pts;
      stats.CA = -(int)player.sca;
      stats.CM = -(int)player.scm;
      stats.CO = -(int)player.sco;
      stats.CR = -(int)player.scr;
      stats.DG = -(int)player.sdg;
      stats.DM = -(int)player.sdm;
      stats.EO = -(int)player.seo;
      stats.ER = -(int)player.ser;
      stats.IO = -(int)player.sio;
      stats.LA = -(int)player.sla;
      stats.LB = -(int)player.slb;
      stats.LC = -(int)player.slc;
      stats.LE = -(int)player.sle;
      stats.LS = -(int)player.sls;
      stats.LT = -(int)player.slt;
      stats.MP = -(int)player.smp;
      stats.PC = -(int)player.spc;
      stats.PG = -(int)player.spg;
      stats.PI = -(int)player.spi;
      stats.RT = -(int)player.srt;
      stats.RU = -(int)player.sru;
      stats.SP = -(int)player.ssp;
      stats.TA = -(int)player.sta;
      stats.TB = -(int)player.stb;
      stats.TC = -(int)player.stc;
      stats.TF = -(int)player.stf;
      stats.TL = -(int)player.stl;
      stats.TM = -(int)player.stm;
      stats.TR = -(int)player.str;
      stats.TS = -(int)player.sts;
      stats.TW = -(int)player.stw;
      stats.MinutesPlayed = -(int)player.tog;
    }

    // initial line ups and stats
    static void LoadGameData(int rnd)
    {
      using (var dc = new RugbySuperCoachDataContext())
      {
        using (var reader = new JsonTextReader(File.OpenText("round" + rnd + "-games.json")))
        {
          dynamic json = new JsonSerializer().Deserialize(reader);
          dynamic fixture = json.fixture;

          foreach (dynamic game in fixture.games)
          {
            var gid = game.id;
            foreach (var player in game.team1_players.players)
            {
              var s = CreateStat(gid, player);
              dc.PlayerStats.InsertOnSubmit(s);
            }

            foreach (var player in game.team2_players.players)
            {
              var s = CreateStat(gid, player);
              dc.PlayerStats.InsertOnSubmit(s);
            }
          }
        }
        dc.SubmitChanges();
      }
    }
    
    static PlayerStat CreateStat(dynamic gid, dynamic player)
    {
      var stats = new PlayerStat
      {
        GameId = gid,
        PlayerId = player.id,
        Value = player.p,
        Total = player.pts,
        CA = player.sca,
        CM = player.scm,
        CO = player.sco,
        CR = player.scr,
        DG = player.sdg,
        DM = player.sdm,
        EO = player.seo,
        ER = player.ser,
        IO = player.sio,
        LA = player.sla,
        LB = player.slb,
        LC = player.slc,
        LE = player.sle,
        LS = player.sls,
        LT = player.slt,
        MP = player.smp,
        PC = player.spc,
        PG = player.spg,
        PI = player.spi,
        RT = player.srt,
        RU = player.sru,
        SP = player.ssp,
        TA = player.sta,
        TB = player.stb,
        TC = player.stc,
        TF = player.stf,
        TL = player.stl,
        TM = player.stm,
        TR = player.str,
        TS = player.sts,
        TW = player.stw
      };

      return stats;
    }

    // fixture data
    static void LoadRoundFixtures(int rnd)
    {
      using (var dc = new RugbySuperCoachDataContext())
      {
        using (var reader = new JsonTextReader(File.OpenText("round" + rnd + "-games.json")))
        {
          dynamic json = new JsonSerializer().Deserialize(reader);
          dynamic fixture = json.fixture;

          foreach (dynamic game in fixture.games)
          {
            var id = (int) game.id;
            var round = game.round;
            var team1 = game.team1;
            var team2 = game.team2;
            var team1score = game.team1_score;
            var team2score = game.team2_score;
            

            var g = dc.Games.SingleOrDefault(x => x.Id == id);

            if (g == null)
            {
              g = new Game { Id = id };
              dc.Games.InsertOnSubmit(g);
            }

            g.Round = round;
            g.HomeTeamId = team1;
            g.AwayTeamId = team2;
            g.HomeScore = team1score;
            g.AwayScore = team2score;

          }
        }

        dc.SubmitChanges();
      }
    }

    // initial load of players
    static void LoadPlayers()
    {
      using (var dc = new RugbySuperCoachDataContext())
      {
        using (var reader = new JsonTextReader(File.OpenText("round1-players.json")))
        {
          dynamic players = new JsonSerializer().Deserialize(reader);

          foreach (var player in players)
          {
            var pid = (int)player.id;
            if (!dc.Players.Any(x => x.Id == pid))
            {
              var p = CreatePlayer(player);
              dc.Players.InsertOnSubmit(p);
            }
            else
            {
              var p = dc.Players.Single(x => x.Id == pid);
              p.InitialValue = -(int)player.p;
            }
          }
        }
        dc.SubmitChanges();
      }
    }

    static Player CreatePlayer(dynamic player)
    {
      var id = player.id;
      var name = player.fn + " " + player.ln;
      var pos = player.pos;
      var team = player.team_id;

      return new Player { Id = id, Name = name, Position = pos, TeamId = team };
    }
  }
}
