using DelBot.Databases;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Timers;
using DelBot.Modules.RPG;

namespace DelBot.Modules {

    public class RPGCommands : ModuleBase<SocketCommandContext> {

        private string cmds = "```>>rpg init {name|default:username} {str|integer} {vit|integer} {int|integer} {dex|integer}\n" +
                                 ">>rpg stats\n" +
                                 ">>rpg attack {@user}```";

        [Command("rpg")]
        public async Task RPGAsync([Remainder]string args = "") {

            string user = Context.User.Mention;
            string userId = "" + Utilities.GetId(user);

            List<string> arglist = Utilities.ParamSplit(args);

            if (arglist.Count == 0) {
                await ReplyAsync("Please enter one of the following arguments:\n" + cmds);
                return;
            }

            RPGProfile profile = RPGProfile.Open(userId);

            if (profile == null) {
                await ReplyAsync("Somehow your profile is already open, " + user + "-dono. Are you sending me multiple commands at once?");
                return;
            }

            string cmd = arglist[0];

            arglist.RemoveAt(0);

            switch (cmd) {
                case "init":
                    string name = Utilities.GetUsername(userId, Context);
                    int s = 0, v = 0, i = 0, d = 0;

                    if ((arglist.Count == 4 || arglist.Count == 5)
                        && int.TryParse(arglist[arglist.Count - 4], out s)
                        && int.TryParse(arglist[arglist.Count - 3], out v)
                        && int.TryParse(arglist[arglist.Count - 2], out i)
                        && int.TryParse(arglist[arglist.Count - 1], out d)) {

                        if (arglist.Count == 5) {
                            name = arglist[0];
                        }

                        if (profile.SetStats(s, v, i, d)) {
                            await ReplyAsync("Profile successfully created. Welcome to the game, " + name + "-san");
                        } else {
                            await ReplyAsync("Your stats are invalid. Please ensure all stats are between 0 and 10 and the total is no more than 20. As punishment, your stats hasve been set to 0. Please try again");
                        }
                    } else {
                        profile.SetStats(5, 5, 5, 5);
                        await ReplyAsync("Try Initiating with the following args:\n`[name] str vit int dex`\nA profile was created for you with default stats. Now that's not very fun.");
                    }

                    profile.name = name;

                    break;

                case "stats":
                    if (arglist.Count == 0) {
                        await ReplyAsync("Here are your stats:" + profile);
                    }
                    break;

                case "attack":
                    if (arglist.Count == 1) {

                        if (Utilities.GetUsername(arglist[0], Context) != null) {
                            RPGProfile oProfile = new RPGProfile("" + Utilities.GetId(arglist[0]));

                            await ReplyAsync(profile.name + " is now attacking " + oProfile.name + "!");

                            if (oProfile != null && oProfile.name != "") {

                                List<string> battleLog = SimulateBattle(profile, oProfile);

                                foreach (string line in battleLog) {
                                    await Program.EnqueueMessage(line, Context.Channel);
                                }

                            }

                        }
                    }
                    break;
                default:
                    break;
            }

            profile.Close();
        }

        private List<string> SimulateBattle(RPGProfile profile, RPGProfile oProfile) {
            profile.SetBattleStats();
            oProfile.SetBattleStats();

            int ahp = profile.hp;
            int ohp = oProfile.hp;
            int abar = 0;
            int obar = 0;

            // Turn#, AttackerTurn, Damage, WeaponDamage, Hit, Evade, Crit
            List<Tuple<int, bool, int, int, bool, bool, bool>> battleCode = new List<Tuple<int, bool, int, int, bool, bool, bool>>();
            List<string> battleLog = new List<string>();

            Random r = new Random();

            while (ahp > 0 && ohp > 0) {
                while (abar < 100 && obar < 100) {
                    abar += profile.agi;
                    obar += oProfile.agi;
                }

                float hit = (float)r.NextDouble();
                float evade = (float)r.NextDouble();
                float crit = (float)r.NextDouble();

                if (abar >= obar) {
                    abar = 0;
                    if (oProfile.eva < evade) {
                        if (profile.acc > hit) {
                            if (profile.crt > crit) {
                                int damage = (int)Math.Ceiling((double)profile.cdm * Math.Max(profile.atk - oProfile.def, 1));
                                ohp -= damage;
                                battleLog.Add(profile.name + " attacked " + oProfile.name + " and landed a critical hit! " + damage + " damage dealt. " + oProfile.name + " has " + ohp + " hp remaining.\n");

                            } else {
                                int damage = Math.Max(profile.atk - oProfile.def, 1);
                                ohp -= damage;
                                battleLog.Add(profile.name + " attacked " + oProfile.name + ". " + damage + " damage dealt. " + oProfile.name + " has " + ohp + " hp remaining.\n");

                            }
                        } else {
                            int damage = (int)Math.Ceiling(0.5 * Math.Max(profile.atk - oProfile.def, 1));
                            ohp -= damage;
                            battleLog.Add(profile.name + " attacked " + oProfile.name + ", but only glanced " + oProfile.name + "! " + damage + " damage dealt. " + oProfile.name + " has " + ohp + " hp remaining.\n");

                        }
                    } else if (profile.acc < hit) {
                        int damage = oProfile.atk;
                        ahp -= damage;
                        battleLog.Add(profile.name + " attacked " + oProfile.name + ", but missed and " + oProfile.name + " parried! " + damage + " damage dealt. " + profile.name + " has " + ahp + " hp remaining.\n");

                    } else {
                        battleLog.Add(profile.name + " attacked " + oProfile.name + ", but missed.\n");

                    }
                } else {
                    obar = 0;
                    if (profile.eva < evade) {
                        if (oProfile.acc > hit) {
                            if (oProfile.crt > crit) {
                                int damage = (int)Math.Ceiling((double)oProfile.cdm * Math.Max(oProfile.atk - profile.def, 1));
                                ahp -= damage;
                                battleLog.Add(oProfile.name + " attacked " + profile.name + " and landed a critical hit! " + damage + " damage dealt. " + profile.name + " has " + ahp + " hp remaining.\n");

                            } else {
                                int damage = Math.Max(oProfile.atk - profile.def, 1);
                                ahp -= damage;
                                battleLog.Add(oProfile.name + " attacked " + profile.name + ". " + damage + " damage dealt. " + profile.name + " has " + ahp + " hp remaining.\n");

                            }
                        } else {
                            int damage = (int)Math.Ceiling(0.5 * Math.Max(oProfile.atk - profile.def, 1));
                            ahp -= damage;
                            battleLog.Add(oProfile.name + " attacked " + profile.name + ", but only glanced " + profile.name + "! " + damage + " damage dealt. " + profile.name + " has " + ahp + " hp remaining.\n");

                        }
                    } else if (oProfile.acc < hit) {
                        int damage = profile.atk;
                        ohp -= damage;
                        battleLog.Add(oProfile.name + " attacked " + profile.name + ", but missed and " + profile.name + " parried! " + damage + " damage dealt. " + oProfile.name + " has " + ohp + " hp remaining.\n");

                    } else {
                        battleLog.Add(oProfile.name + " attacked " + profile.name + ", but missed.\n");

                    }
                }
            }


            if (ahp <= 0) {
                battleLog.Add(profile.name + " has fainted. Victory goes to " + oProfile.name);
            } else {
                battleLog.Add(oProfile.name + " has fainted. Victory goes to " + profile.name);
            }

            return battleLog;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}
