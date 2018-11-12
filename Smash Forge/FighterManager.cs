using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Smash_Forge
{
    public class FighterManager
    {
        public string name { get; private set; }
        public int ID { get; private set; } = -1;
        public ModelContainer model { get; set; }
        //animation?
        public MovesetManager moveset { get; set; }

        public FighterManager(string name)
        {
            this.name = name;
            if ((ID = fighters.FindIndex(ft => ft == name)) < 0) throw new Exception(name + " is not a fighter");
        }
        public FighterManager(DirectoryInfo directoryInfo)
        {
            name = directoryInfo.Name;
            if ((ID = fighters.FindIndex(ft => ft == name)) < 0) throw new Exception(name + " is not a fighter");
            foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
            {
                switch (subDir.Name)
                {
                    case "camera"://unimplemented
                        continue;
                    case "effect"://unimplemented
                        continue;
                    case "model":
                        {
                            //later on, allow choosing cXX folder manually
                            model = new ModelContainer(Directory.GetFiles(subDir.FullName + "\\body\\c00"));
                        }
                        continue;
                    case "motion":
                        continue;
                    case "script":
                        continue;
                    case "sound"://unimplemented
                        continue;
                }
            }
        }

        static List<string> fighters = new List<string> { "miifighter", "miiswordsman", "miigunner", "mario", "donkey", "link", "samus", "yoshi", "kirby", "fox", "pikachu", "luigi", "captain", "ness", "peach", "koopa", "zelda", "sheik", "marth", "gamewatch", "ganon", "falco", "wario", "metaknight", "pit", "szerosuit", "pikmin", "diddy", "dedede", "ike", "lucario", "robot", "toonlink", "lizardon", "sonic", "purin", "mariod", "lucina", "pitb", "rosetta", "wiifit", "littlemac", "murabito", "palutena", "reflet", "duckhunt", "koopajr", "shulk", "gekkouga", "pacman", "rockman", "mewtwo", "ryu", "lucas", "roy", "cloud", "bayonetta", "kamui", "koopag", "warioman", "littlemacg", "lucariom", "miienemyf", "miienemys", "miienemyg" };
    }

}
