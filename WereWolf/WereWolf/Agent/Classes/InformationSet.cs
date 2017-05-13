﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WereWolf.Nodes;

namespace WereWolf
{
    public class InformationSet
    {
        private List<String> players;
        private Dictionary<String, List<String>> accusedPlayers;
        private List<String> savedPeople;
        private string playerName;
        private List<string> friends;
        private Dictionary<String, PlayerBelief> beliefsPerPlayer;
        private bool liar;

        //Dummy agent
        Random rnd;

        public InformationSet(string playerName, bool isLiar)
        {
            players = new List<string>();
            rnd = new Random(Guid.NewGuid().GetHashCode());
            friends = new List<string>();
            accusedPlayers = new Dictionary<String, List<String>>();
            beliefsPerPlayer = new Dictionary<string, PlayerBelief>();
            savedPeople = new List<string>();
            this.playerName = playerName;
            liar = isLiar;
        }

        public void addAccusePlay(String playerName, String accusedName)
        {
            //Add accused playing
            List<String> accusedList;
            if (accusedPlayers.TryGetValue(playerName, out accusedList))
            {
                accusedList.Add(accusedName);
                accusedPlayers[playerName] = accusedList;
            }
            else
            {
                accusedPlayers.Add(playerName, new List<string> { accusedName });
            }
        }

        public void addSeerAnswer(String playerName, String roleName)
        {
            beliefsPerPlayer[playerName].addRole(roleName);
        }

        public void addKillPlay(String playerName)
        {
            players.Remove(playerName);
        }

        public void addFriend(string friend)
        {
            friends.Add(friend);
            beliefsPerPlayer[friend].addRole("Werewolf");
        }

        public void addTalk(string talker, string playerName, string role)
        {
            if(players.Contains(playerName) && players.Contains(talker))
                beliefsPerPlayer[talker].addLog(playerName, role);
        }

        public void addSave(string playerName)
        {
            if(!savedPeople.Contains(playerName))
                savedPeople.Add(playerName);
        }

        public void addRole(String playerName, String playerRole)
        {
            if (beliefsPerPlayer.ContainsKey(playerName))
            {
                beliefsPerPlayer[playerName].addRole(playerRole);
            }
        }

        public void updateBeliefs()
        {
            foreach (String player in beliefsPerPlayer.Keys)
                if (!player.Equals(playerName))
                {
                    beliefsPerPlayer[player].updateBeliefs(players, accusedPlayers, savedPeople, beliefsPerPlayer);
                }
            accusedPlayers.Clear();
        }

        public void setPlayersList(List<String> p)
        {
            players = new List<String>(p);
            foreach (String name in players)
            {
                if(!name.Equals(playerName))
                    beliefsPerPlayer.Add(name, new PlayerBelief(name));
            }
        }

        public List<PlayerNode> Sample()
        {
            //TODO
            //Update beliefs based on accuses
            List<PlayerNode> accuseSample = new List<PlayerNode>(players.Count);

            //Lets infer first information - Player will not accuse himself
            foreach (string player in players)
            {
                bool isRoleDecided = false;
                if (player.Equals(playerName)) continue;

                PlayerBelief playerBelief;

                if (beliefsPerPlayer.TryGetValue(player, out playerBelief))
                {
                    int percentageSuccess = rnd.Next(100);
                    Tuple<string,float> roleBelief = playerBelief.getRole();
                    if (percentageSuccess <= roleBelief.Item2)
                    {
                        accuseSample.Add(new RuleBasedNode(player, roleBelief.Item1, this));
                        isRoleDecided = true;
                    }
                }
                if(!isRoleDecided)
                {
                    int randomNumber = -1;
                    if (friends.Count > 0)
                        randomNumber = rnd.Next(1, 4);
                    else randomNumber = rnd.Next(4);

                    if (randomNumber == 1)
                    {
                        accuseSample.Add(new RuleBasedNode(player , "Villager", this));
                    }
                    if (randomNumber == 2)
                    {
                        accuseSample.Add(new RuleBasedNode(player, "Seer", this));
                    }
                    if (randomNumber == 3)
                    {
                        accuseSample.Add(new RuleBasedNode(player, "Doctor", this));
                    }
                    if (randomNumber == 0)
                    {
                        accuseSample.Add(new RuleBasedNode(player, "Werewolf", this));
                    }
                }
            }
            return accuseSample;
        }

        public string ruledBasedAccuse()
        {
            List<String> accuseList = players.Where(p => p != playerName).ToList();
            if (accuseList.Count > 0)
                return string.Format("accuse {0}", accuseList[rnd.Next(accuseList.Count)]);
            else return string.Empty;
        }

        public string questionSample()
        {
            return players[rnd.Next(players.Count)];
        }

        public Dictionary<String, int> getPossibleTalks()
        {
            Dictionary<String, int> possibleTalks = new Dictionary<String, int>();
            if (beliefsPerPlayer.Count > 0)
            {
                foreach (KeyValuePair<string, PlayerBelief> playerBelief in beliefsPerPlayer)
                {
                    Tuple<string, float> belief = playerBelief.Value.getRole();

                    if (!players.Contains(playerBelief.Key)) continue;
                    if (belief.Item2 >= 100 && !liar)
                        possibleTalks.Add(string.Format("talk The player {0} is a {1}", playerBelief.Key, belief.Item1), 0);
                    else if(belief.Item2 < 100 && liar)
                        possibleTalks.Add(string.Format("talk The player {0} is a {1}", playerBelief.Key, belief.Item1), 0);
                }
            }

            if(possibleTalks.Count == 0) possibleTalks.Add("talk I don't know", 0);

            return possibleTalks;
        }

        public Dictionary<String, int> getPossibleAccuses()
        {
            return players.Select(x => x).Where(x => x != playerName && !friends.Contains(x)).ToDictionary(x => string.Format("accuse {0}",x) , x => 0);
        }

        public Dictionary<String, int> getPossibleKills()
        {
            return players.Select(x => x).Where(x => x != playerName && !friends.Contains(x)).ToDictionary(x => string.Format("kill {0}", x), x => 0);
        }

        public Dictionary<String, int> getPossibleQuestions()
        {
            return players.Select(x => x).Where(x => x != playerName).ToDictionary(x => string.Format("question {0}", x), x => 0);
        }

        public Dictionary<String, int> getPossibleHeals()
        {
            return players.Select(x => x).Where(x => x != playerName).ToDictionary(x => string.Format("heal {0}", x), x => 0);
        }
    }
}
