using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TEXT_RPG.Models;

namespace TEXT_RPG
{
    delegate bool SkillDelegate(string SkillClass, string MonsterName, string CharacterName, ref int CharacterMP, int Def, ref int Hp, ref int HP, ref int MonsterEXP);

    internal class TEXT_RPG2
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

        static void Main(string[] args)
        {
            string Name;
            bool CheckCharacterBeing = false;
            Character character = new Character();
            Monster Monster = new Monster();

            Console.Write("請輸入角色名稱:");
            Name = Console.ReadLine();
            CheckCharacterBeing = character.CheckCharacter(Name, CheckCharacterBeing);
            if (!CheckCharacterBeing)
            {
                character.CreateCharacter(Name);
            }
            while (true)
            {
                character.ShowCharacter();
                Monster.ShowMonster();
                Monster.AttackMonster();
            }
        }
        public void ExitGame()
        {
            Console.Write("確定要離開遊戲?(Y/N)");
            string check = Console.ReadLine();
            if (check == "Y")
            {
                using (RpgDataContext db = new RpgDataContext(new DbContextOptionsBuilder<RpgDataContext>().UseSqlServer(configuration.GetConnectionString("CS_RPG")).Options))
                {
                    Character character = new Character();
                    bool check_bool = false;
                    CharacterData CharData = new CharacterData();
                    foreach (var database in character.CharData)
                    {
                        foreach (var save in db.CharacterData.Where(x => x.Name == database.Name))
                        {
                            save.LV = database.LV;
                            save.Name = database.Name;
                            save.Job = database.Job;
                            save.HP = database.HP;
                            save.MP = database.MP;
                            save.ATK = database.ATK;
                            save.DEF = database.DEF;
                            save.STR = database.STR;
                            save.VIT = database.VIT;
                            save.AGI = database.AGI;
                            save.INT = database.INT;
                            save.LUK = database.LUK;
                            save.EXP = database.EXP;
                            db.SaveChanges();
                            check_bool = true;
                        }
                        if (!check_bool)
                        {
                            db.CharacterData.AddRange(character.CharData);
                            db.SaveChanges();
                        }
                    }
                }
                Console.Write("遊戲結束");
                System.Threading.Thread.Sleep(500);
                Environment.Exit(Environment.ExitCode);
            }
        }
    }


    public class Character
    {
        private IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();
        private static List<CharacterData> charData;
        public List<CharacterData> CharData { get { return charData; } set { charData = value; } }
        public void CreateCharacter(string Name)
        {

            Job Job = new Job();
            string Chouse = "N", JobName = "";

            while (Chouse != "Y")
            {
                Job.ShowJob();
                Console.Write("\n\n請輸入你選擇的職業名稱:");
                JobName = Console.ReadLine();
                Job.ShowJobDetailed(JobName);
                Chouse = Console.ReadLine();
            }
            using (RpgDataContext db = new RpgDataContext(new DbContextOptionsBuilder<RpgDataContext>().UseSqlServer(configuration.GetConnectionString("CS_RPG")).Options))
            {
                var GetCharacterData = from CharacterData in db.CharacterData
                                       select CharacterData;
                CharData = new List<CharacterData>();

                foreach (var JobData in Job.JobDatas.Where(x => x.JobName == JobName))
                {
                    Dictionary<string, int> JobForAtk = new Dictionary<string, int>
                    {
                        {"劍士",JobData.STR},
                        {"法師",JobData.INT},
                        {"弓箭手",JobData.AGI},
                        {"刺客",JobData.LUK},
                        {"補師",JobData.INT/2}
                    };
                    CharacterData Character = new CharacterData()
                    {
                        Name = Name,
                        Job = JobData.JobName,
                        LV = 1,
                        EXP = 0,
                        HP = JobData.HP,
                        MP = JobData.MP,
                        ATK = JobForAtk[JobData.JobName],
                        DEF = JobData.VIT,
                        STR = JobData.STR,
                        VIT = JobData.VIT,
                        AGI = JobData.AGI,
                        INT = JobData.INT,
                        LUK = JobData.LUK
                    };

                    CharData.Add(Character);
                }
            };

        }

        public void ShowCharacter()
        {
            Skill skill = new Skill();
            foreach (var CharData in CharData)
            {
                Console.WriteLine("\n{0}\t職業:{1}\t等級{2}\t經驗值:{3}\nHP:{4}\tMP:{5}\t攻擊力:{6}\t防禦力:{7}\n力量:{8}\t體質:{9}\t敏捷:{10}\t智力:{11}\t幸運:{12}",
                                      CharData.Name, CharData.Job, CharData.LV, CharData.EXP, CharData.HP, CharData.MP, CharData.ATK, CharData.DEF, CharData.STR, CharData.VIT, CharData.AGI,
                                      CharData.INT, CharData.LUK);
                skill.ShowSkill(CharData.Job);
            }
        }

        public string ShowCharacterFight()
        {
            string CharStr = "";
            foreach (var CharData in CharData)
            {
                CharStr = "等級" + CharData.LV +
                          "\t" + CharData.Name +
                          "\tHP:" + CharData.HP +
                          "\tMP:" + CharData.MP +
                          "\t攻擊力:" + CharData.ATK +
                          "\t防禦力:" + CharData.DEF +
                          "\t經驗值:" + CharData.EXP;
            }
            return CharStr;
        }

        public bool CheckCharacter(string Name, bool CheckCharacterBeing)
        {
            using (RpgDataContext db = new RpgDataContext(new DbContextOptionsBuilder<RpgDataContext>().UseSqlServer(configuration.GetConnectionString("CS_RPG")).Options))
            {
                string checkYN = "N";
                foreach (var CheckCharacter in db.CharacterData.Where(x => x.Name == Name))
                {
                    Console.Write("偵測到{0}角色已存在，是否要讀取?(Y/N):", CheckCharacter.Name);
                    checkYN = Console.ReadLine();
                    if (checkYN == "Y" || checkYN == "y")
                    {
                        CharData = new List<CharacterData>();
                        CharData.Add(CheckCharacter);
                        CheckCharacterBeing = true;

                    }
                    else if (checkYN == "N" || checkYN == "n")
                    {
                        Console.Write("是否覆蓋{0}?(Y/N)", Name);
                        checkYN = Console.ReadLine();
                        if (checkYN.ToUpper() == "Y")
                        {
                            return CheckCharacterBeing;
                        }
                        else if (checkYN.ToUpper() == "N")
                        {
                            CheckCharacterBeing = true;
                        }
                    }
                }
                return CheckCharacterBeing;
            }

        }
        public void CharacterLevelUp()
        {
            using (RpgDataContext db = new RpgDataContext(new DbContextOptionsBuilder<RpgDataContext>().UseSqlServer(configuration.GetConnectionString("CS_RPG")).Options))
            {

                var Exp = from ExpList in db.ExpForm
                          select ExpList;
                int HPUP = 0, MPUP = 0, STRUP = 0, VITUP = 0, AGIUP = 0, INTUP = 0, LUKUP = 0, ATKUP = 0, DEFUP = 0;
                Dictionary<string, JobLevelUp> _LevelUpList;
                foreach (var CharData in CharData)
                {
                    foreach (var Level in Exp.Where(x => x.LV > CharData.LV))
                    {
                        if (CharData.EXP >= Level.EXP)
                        {
                            _LevelUpList = new Dictionary<string, JobLevelUp>()
                            {
                                {"劍士",new SwordMan() },
                                {"法師",new Mage() },
                                {"弓箭手",new Archer() },
                                {"刺客",new Assassin() },
                                {"補師",new Monk() }
                            };
                            _LevelUpList[CharData.Job].LevelUp(HPUP, MPUP, ATKUP, DEFUP, STRUP, VITUP, AGIUP, INTUP, LUKUP, CharData);
                        }
                        else if (CharData.LV == Level.LV - 1)
                        {
                            Console.WriteLine("距離升等還差{0}點經驗", Level.EXP - CharData.EXP);
                        }
                    }
                }

            }
        }
    }
    public class Job
    {
        private IConfigurationRoot configuration = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json")
       .Build();
        private List<JobData> jobDatas;
        public List<JobData> JobDatas { get { return jobDatas; } set { jobDatas = value; } }

        public void ShowJob()
        {
            using (RpgDataContext db = new RpgDataContext(new DbContextOptionsBuilder<RpgDataContext>().UseSqlServer(configuration.GetConnectionString("CS_RPG")).Options))
            {
                var GetJobData = from JobData in db.JobData
                                 select JobData;
                JobDatas = new List<JobData>();
                foreach (var Job in GetJobData)
                {
                    JobData JobDataTable = new JobData()
                    {
                        JobName = Job.JobName,
                        HP = Job.HP,
                        MP = Job.MP,
                        STR = Job.STR,
                        VIT = Job.VIT,
                        AGI = Job.AGI,
                        INT = Job.INT,
                        LUK = Job.LUK
                    };
                    JobDatas.Add(JobDataTable);
                }
                Console.WriteLine("職業列表\n");
                foreach (var Job in JobDatas)
                {
                    Console.Write(Job.JobName + "\t");
                }
            }

        }
        public void ShowJobDetailed(string JobName)
        {
            bool check = false;
            Skill Skill = new Skill();
            while (check == false)
            {
                foreach (var Job in JobDatas.Where(x => x.JobName == JobName))
                {
                    Console.WriteLine("\n{0}\nHP:{1}\tMP:{2}\t力量:{3}\t敏捷:{4}\t智力:{5}\t幸運:{6}\t",
                                      Job.JobName, Job.HP, Job.MP, Job.STR, Job.AGI, Job.INT, Job.LUK);
                    Skill.ShowSkill(JobName);
                    check = true;
                }
                if (!check)
                {
                    Console.Write("您輸入的職業不存在請重新輸入:");
                    JobName = Console.ReadLine();
                }
                else
                {
                    Console.Write("\n確定選擇此職業?(Y/N):");
                }
            }
        }
    }
    public class Skill
    {
        private IConfigurationRoot configuration = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json")
       .Build();
        private static List<SkillData> skillDatas;
        public List<SkillData> SkillDatas { get { return skillDatas; } set { skillDatas = value; } }
        public void ShowSkill(string JobName)
        {
            using (RpgDataContext db = new RpgDataContext(new DbContextOptionsBuilder<RpgDataContext>().UseSqlServer(configuration.GetConnectionString("CS_RPG")).Options))
            {
                var ShowSkill = from ShowS in db.SkillData.Where(x => x.SkillClass == JobName)
                                select ShowS;
                SkillDatas = new List<SkillData>();
                foreach (var Show in ShowSkill)
                {
                    SkillData SkillDataTable = new SkillData()
                    {
                        SkillName = Show.SkillName,
                        SkillInfo = Show.SkillInfo,
                        SkillClass = Show.SkillClass,
                        SkillType = Show.SkillType
                    };
                    SkillDatas.Add(SkillDataTable);
                    Console.WriteLine("{0},{1}", Show.SkillName, Show.SkillInfo);
                }
            }
        }
        public static bool AttackSkill(string SkillClass, string MonsterName, string CharacterName, ref int CharacterMP, int MonsterDef, ref int MonsterHp, ref int CharacterHP, ref int MonsterEXP)
        {
            Character character = new Character();
            Skill skill = new Skill();
            Monster monster = new Monster();
            CharacterData Character = new CharacterData();
            int MaxMp = 0;
            bool AttackSkillEvent = false;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            foreach (var CharacterInfo in character.CharData)
            {
                Character.Job = CharacterInfo.Job;
                Character.HP = CharacterInfo.HP;
                Character.MP = CharacterInfo.MP;
                Character.Name = CharacterInfo.Name;
                Character.ATK = CharacterInfo.ATK;
                Character.STR = CharacterInfo.STR;
                Character.VIT = CharacterInfo.VIT;
                Character.AGI = CharacterInfo.AGI;
                Character.INT = CharacterInfo.INT;
                Character.LUK = CharacterInfo.LUK;
                MaxMp = CharacterInfo.MP;
            }
            foreach (var SkillList in skill.SkillDatas.Where(x => x.SkillClass == SkillClass && (x.SkillType == "主動攻擊" || x.SkillType == "被動攻擊")))
            {
                int MP;
                switch (SkillList.SkillName)
                {
                    case "火球":
                        MP = 10;
                        if (CharacterMP >= MP)
                        {
                            Character.ATK = Character.INT * 3;
                            if (Crit(Character.ATK))
                            {
                                Character.ATK *= 2;
                                Console.WriteLine("{0}對{1}發動火球!，觸發了暴擊，造成{2}點傷害,MP消耗{3}",
                                             CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef), MP);
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                CharacterMP -= MP;
                                AttackSkillEvent = true;
                            }
                            else
                            {
                                Console.WriteLine("{0}對{1}發動火球!，造成{2}點傷害,MP消耗{3}",
                                              CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef), MP);
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                CharacterMP -= MP;
                                AttackSkillEvent = true;
                            }

                        }
                        break;
                    case "劍氣":
                        MP = MaxMp / 4;
                        if (rnd.Next(1, 100) <= 30 && CharacterMP >= MP)
                        {
                            Character.ATK *= 3;
                            if (Crit(Character.ATK))
                            {
                                Character.ATK *= 2;
                                Console.WriteLine("{0}對{1}發動劍氣!，觸發了暴擊，造成{2}點傷害,MP消耗{3}",
                                              CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef), MP);
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                CharacterMP -= MP;
                                AttackSkillEvent = true;
                            }
                            else
                            {
                                Console.WriteLine("{0}對{1}發動劍氣!，造成{2}點傷害,MP消耗{3}",
                                              CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef), MP);
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                CharacterMP -= MP;
                                AttackSkillEvent = true;
                            }

                        }
                        break;
                    case "暴擊":
                        if (Crit(Character.ATK))
                        {
                            Console.WriteLine("{0}對{1}發動攻擊!觸發了暴擊，造成{2}點傷害",
                                CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef));
                            MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                            AttackSkillEvent = true;
                        }
                        break;
                    case "懲戒":
                        MP = 10;
                        if (CharacterMP >= MP)
                        {
                            Character.ATK = Character.INT * 2;
                            if (Crit(Character.ATK))
                            {
                                Character.ATK *= 2;
                                Console.WriteLine("{0}對{1}發動懲戒!，觸發了暴擊，造成{2}點傷害,MP消耗{3}",
                                             CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef), MP);
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                CharacterMP -= MP;
                                AttackSkillEvent = true;
                            }
                            else
                            {
                                Console.WriteLine("{0}對{1}發動懲戒!，造成{2}點傷害,MP消耗{3}",
                                              CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef), MP);
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                CharacterMP -= MP;
                                AttackSkillEvent = true;
                            }
                        }
                        break;
                    case "穿雲箭":
                        MP = 20;
                        if (CharacterMP >= MP)
                        {
                            Character.ATK *= 2;
                            if (Crit(Character.ATK))
                            {
                                Character.ATK *= 2;
                                Console.WriteLine("{0}對{1}發動穿雲箭!，觸發了暴擊，造成{2}點傷害,MP消耗{3}",
                                             CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef), MP);
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                CharacterMP -= MP;
                                AttackSkillEvent = true;
                            }
                            else
                            {
                                Console.WriteLine("{0}對{1}發動穿雲箭!，造成{2}點傷害,MP消耗{3}",
                                              CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef), MP);
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                CharacterMP -= MP;
                                AttackSkillEvent = true;
                            }
                        }
                        break;
                    case "蓄力":
                        if (rnd.Next(1, 100) <= 10)
                        {
                            Character.ATK *= 5;
                            if (Crit(Character.ATK))
                            {
                                Character.ATK *= 2;
                                Console.WriteLine("{0}對{1}發動蓄力!，觸發了暴擊，造成{2}點傷害",
                                             CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef));
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                AttackSkillEvent = true;
                            }
                            else
                            {
                                Console.WriteLine("{0}對{1}發動蓄力!，造成{2}點傷害",
                                              CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef));
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                AttackSkillEvent = true;
                            }
                        }
                        break;
                    case "背刺":
                        if (rnd.Next(1, 100) <= 10)
                        {
                            Character.ATK *= 5;
                            if (Crit(Character.ATK))
                            {
                                Character.ATK *= 2;
                                Console.WriteLine("{0}對{1}發動背刺!，觸發了暴擊，造成{2}點傷害",
                                             CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef));
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                AttackSkillEvent = true;
                            }
                            else
                            {
                                Console.WriteLine("{0}對{1}發動背刺!，造成{2}點傷害",
                                              CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef));
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                AttackSkillEvent = true;
                            }
                        }
                        break;
                    case "破防":
                        if (rnd.Next(1, 100) <= 20)
                        {
                            MonsterDef /= 2;
                            if (Crit(Character.ATK))
                            {
                                Character.ATK *= 2;
                                Console.WriteLine("{0}對{1}發動破防!，觸發了暴擊，造成{2}點傷害",
                                             CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef));
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                AttackSkillEvent = true;
                            }
                            else
                            {
                                Console.WriteLine("{0}對{1}發動破防!，造成{2}點傷害",
                                              CharacterName, MonsterName, monster.CountDamage(Character.ATK, MonsterDef));
                                MonsterHp -= monster.CountDamage(Character.ATK, MonsterDef);
                                AttackSkillEvent = true;
                            }
                        }
                        break;
                }
            }
            return AttackSkillEvent;
        }

        public static bool SubsidySkill(string SkillClass, string MonsterName, string CharacterName, ref int CharacterMP, int CharacterDef, ref int CharacterHp, ref int MonsterHP, ref int MonsterEXP)
        {
            bool SubsidySkillEvent = false;
            Character character = new Character();
            Skill skill = new Skill();
            Monster monster = new Monster();
            CharacterData Character = new CharacterData();
            MonsterData Monster = new MonsterData();
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            foreach (var CharacterBattle in character.CharData.Where(x => x.Name == CharacterName))
            {
                Character.ATK = CharacterBattle.ATK;
                Character.HP = CharacterBattle.HP;
            }
            foreach (var MonsterBattle in monster.MonsterDatas.Where(x => x.Name == MonsterName))
            {
                Monster.ATK = MonsterBattle.ATK;
                Monster.DEF = MonsterBattle.DEF;
                Monster.EXP = MonsterBattle.EXP;
            }
            int MonsterRATK = monster.CountDamage(Monster.ATK, CharacterDef);

            foreach (var SkillList in skill.SkillDatas.Where(x => x.SkillClass == SkillClass && (x.SkillType == "被動輔助" || x.SkillType == "反擊技能" || x.SkillType == "主動補助")))
            {

                switch (SkillList.SkillName)
                {
                    case "魔盾":
                        if (CharacterMP >= MonsterRATK)
                        {
                            Console.WriteLine("{0}對{1}發動攻擊!，技能魔盾發生效果!，造成{2}點MP損失",
                                MonsterName, CharacterName, MonsterRATK);
                            CharacterMP = monster.CountDamage(CharacterMP, MonsterRATK);
                            SubsidySkillEvent = true;
                        }
                        break;
                    case "鋼鐵":
                        Console.WriteLine("{0}對{1}發動攻擊!，被動技能鋼鐵發動只造成{2}點傷害",
                            MonsterName, CharacterName, (MonsterRATK / 2));
                        CharacterHp -= (MonsterRATK / 2);
                        SubsidySkillEvent = true;
                        break;
                    case "以眼還眼":
                        if (rnd.Next(1, 100) <= 20)
                        {
                            Console.WriteLine("{0}對{1}發動攻擊!，造成{2}點傷害",
                            MonsterName, CharacterName, MonsterRATK);
                            CharacterHp -= MonsterRATK;
                            Console.WriteLine("以眼還眼觸發! {0}對{1}反擊，造成{2}點傷害",
                                               CharacterName, MonsterName, monster.CountDamage(Monster.ATK + Character.ATK, Monster.DEF));
                            MonsterHP -= monster.CountDamage(Monster.ATK + Character.ATK, Monster.DEF);
                            SubsidySkillEvent = true;
                        }
                        break;
                    case "虛幻":
                        if (rnd.Next(1, 100) <= 20)
                        {
                            Console.WriteLine("{0}對{1}發動攻擊!，被動技能虛幻發動沒有造成傷害!",
                            MonsterName, CharacterName);
                            SubsidySkillEvent = true;
                        }
                        break;
                    case "補血":
                        if (CharacterHp < Character.HP)
                        {
                            Console.WriteLine("{0}對{1}發動攻擊!，造成{2}點傷害",
                            MonsterName, CharacterName, MonsterRATK);
                            CharacterHp -= MonsterRATK;
                            Console.WriteLine("發動補血技能，回復{0}點血量", Character.INT * 3);
                            if ((CharacterHp += Character.INT * 3) >= Character.HP)
                            {
                                CharacterHp = Character.HP;

                            }
                            else
                            {
                                CharacterHp += Character.INT * 3;
                            }
                            SubsidySkillEvent = true;

                        }
                        break;
                    case "祈禱":
                        if (MonsterEXP == Monster.EXP)
                        {
                            Console.WriteLine("發動祈禱技能，怪物經驗值X2");
                            MonsterEXP *= 2;
                            SubsidySkillEvent = true;
                        }
                        break;
                }
            }
            return SubsidySkillEvent;
        }
        public static bool Crit(int ATK)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            bool CritEvent = false;
            if (rnd.Next(1, 100) <= 20)
            {
                CritEvent = true;
                return CritEvent;
            }
            return CritEvent;
        }
    }
    public class Monster
    {
        private IConfigurationRoot configuration = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json")
       .Build();
        private static List<MonsterData> monsterDatas;
        public List<MonsterData> MonsterDatas { get { return monsterDatas; } set { monsterDatas = value; } }
        public void ShowMonster()
        {
            using (RpgDataContext db = new RpgDataContext(new DbContextOptionsBuilder<RpgDataContext>().UseSqlServer(configuration.GetConnectionString("CS_RPG")).Options))
            {
                var ShowMonster = from ShowM in db.MonsterData
                                  select ShowM;
                Console.WriteLine("\n\n怪物列表");
                MonsterDatas = new List<MonsterData>();
                foreach (var Show in ShowMonster)
                {
                    MonsterData MonsterDataTable = new MonsterData()
                    {
                        LV = Show.LV,
                        Name = Show.Name,
                        HP = Show.HP,
                        ATK = Show.ATK,
                        DEF = Show.DEF,
                        EXP = Show.EXP
                    };
                    MonsterDatas.Add(MonsterDataTable);
                    Console.WriteLine("等級:{0}\t{1}\tHP:{2}\t攻擊力:{3}\t防禦力:{4}\t經驗值:{5}",
                                      Show.LV, Show.Name, Show.HP, Show.ATK, Show.DEF, Show.EXP);
                }
            }
        }

        public void AttackMonster()
        {
            string MonsterName;
            bool GetMonster = false;
            int CharHP = 0, CharMP = 0, MonsterHP = 0;

            Character character = new Character();
            while (!GetMonster)
            {
                Console.Write("\n請輸入要攻擊的怪物或輸入Q離開遊戲:");
                MonsterName = Console.ReadLine();
                if (MonsterName == "Q")
                {
                    TEXT_RPG2 Main = new TEXT_RPG2();
                    Main.ExitGame();
                }
                CharacterData CharData = new CharacterData();
                foreach (var Show in character.CharData)
                {
                    CharData.LV = Show.LV;
                    CharData.Name = Show.Name;
                    CharData.Job = Show.Job;
                    CharData.HP = Show.HP;
                    CharData.MP = Show.MP;
                    CharData.ATK = Show.ATK;
                    CharData.DEF = Show.DEF;
                    CharData.STR = Show.STR;
                    CharData.VIT = Show.VIT;
                    CharData.AGI = Show.AGI;
                    CharData.INT = Show.INT;
                    CharData.LUK = Show.LUK;
                    CharData.EXP = Show.EXP;
                    CharHP = Show.HP;
                    CharMP = Show.MP;
                }
                MonsterData MonsterData = new MonsterData();
                foreach (var Chouse in MonsterDatas.Where(x => x.Name == MonsterName))
                {
                    MonsterData = Chouse;
                }

                Console.WriteLine("\n等級:{0}\t{1}\tHP:{2}\tMP:{3}\t攻擊力:{4}\t防禦力:{5}\t經驗值:{6}",
                                 CharData.LV, CharData.Name, CharData.HP, CharData.MP, CharData.ATK, CharData.DEF, CharData.EXP);
                Console.WriteLine("等級:{0}\t{1}\tHP:{2}\t攻擊力:{3}\t防禦力:{4}\t經驗值:{5}\n",
                                  MonsterData.LV, MonsterData.Name, MonsterData.HP, MonsterData.ATK, MonsterData.DEF, MonsterData.EXP);
                MonsterHP = MonsterData.HP;
                while (GetMonster == false)
                {
                    int CharDataHP = CharData.HP,
                        CharDataMP = CharData.MP,
                        ChouseHP = MonsterData.HP,
                        ChouseEXP = MonsterData.EXP;
                    SkillDelegate AttackSkill = new SkillDelegate(Skill.AttackSkill);
                    if (AttackSkill(CharData.Job, MonsterData.Name, CharData.Name, ref CharDataMP, MonsterData.DEF, ref ChouseHP, ref CharDataHP, ref ChouseEXP))
                    {
                        MonsterData.HP = ChouseHP;
                        CharData.HP = CharDataHP;
                        CharData.MP = CharDataMP;
                    }
                    else
                    {
                        Console.WriteLine("{0}對{1}發動攻擊!造成{2}點傷害", CharData.Name, MonsterData.Name, CountDamage(CharData.ATK, MonsterData.DEF));
                        MonsterData.HP -= CountDamage(CharData.ATK, MonsterData.DEF);
                        ChouseHP = MonsterData.HP;
                    }
                    SkillDelegate SubsidySkill = new SkillDelegate(Skill.SubsidySkill);
                    if (SubsidySkill(CharData.Job, MonsterData.Name, CharData.Name, ref CharDataMP, CharData.DEF, ref CharDataHP, ref ChouseHP, ref ChouseEXP))
                    {
                        MonsterData.HP = ChouseHP;
                        CharData.HP = CharDataHP;
                        CharData.MP = CharDataMP;
                        MonsterData.EXP = ChouseEXP;
                    }
                    else
                    {
                        Console.WriteLine("{0}對{1}發動攻擊!造成{2}點傷害", MonsterData.Name, CharData.Name, CountDamage(MonsterData.ATK, CharData.DEF));
                        CharData.HP -= CountDamage(MonsterData.ATK, CharData.DEF);
                    }

                    System.Threading.Thread.Sleep(500);

                    Console.WriteLine("\n等級:{0}\t{1}\tHP:{2}\tMP:{3}\t攻擊力:{4}\t防禦力:{5}\t經驗值:{6}",
                                  CharData.LV, CharData.Name, CharData.HP, CharData.MP, CharData.ATK, CharData.DEF, CharData.EXP);
                    Console.WriteLine("等級:{0}\t{1}\tHP:{2}\t攻擊力:{3}\t防禦力:{4}\t經驗值:{5}\n",
                                  MonsterData.LV, MonsterData.Name, MonsterData.HP, MonsterData.ATK, MonsterData.DEF, MonsterData.EXP);

                    if (MonsterData.HP <= 0 || CharData.HP <= 0)
                    {
                        if (MonsterData.HP <= 0)
                        {
                            Console.WriteLine("\n{0}被打敗!，獲得{1}點經驗!", MonsterData.Name, MonsterData.EXP);
                            MonsterData.EXP = ChouseEXP;
                            CharData.EXP += MonsterData.EXP;
                            GetMonster = true;
                        }
                        else
                        {
                            Console.WriteLine("\n您已被打敗，請再接再厲!");
                            GetMonster = true;
                        }
                    }
                }
                CharData.HP = CharHP;
                CharData.MP = CharMP;
                foreach (var Show in character.CharData)
                {
                    Show.LV = CharData.LV;
                    Show.Name = CharData.Name;
                    Show.Job = CharData.Job;
                    Show.HP = CharData.HP;
                    Show.MP = CharData.MP;
                    Show.ATK = CharData.ATK;
                    Show.DEF = CharData.DEF;
                    Show.STR = CharData.STR;
                    Show.VIT = CharData.VIT;
                    Show.AGI = CharData.AGI;
                    Show.INT = CharData.INT;
                    Show.LUK = CharData.LUK;
                    Show.EXP = CharData.EXP;
                }
                character.CharacterLevelUp();
                using (RpgDataContext db = new RpgDataContext(new DbContextOptionsBuilder<RpgDataContext>().UseSqlServer(configuration.GetConnectionString("CS_RPG")).Options))
                {
                    //var data = RpgDatabase.CharacterDatas.Where(x => x.Name == CharData.Name);
                    //data = CharData;
                    CharacterData data = db.CharacterData.Where(x => x.Name == CharData.Name).FirstOrDefault();

                    bool check_bool = false;
                 
                        data.LV = CharData.LV;
                        data.Name = CharData.Name;
                        data.Job = CharData.Job;
                        data.HP = CharData.HP;
                        data.MP = CharData.MP;
                        data.ATK = CharData.ATK;
                        data.DEF = CharData.DEF;
                        data.STR = CharData.STR;
                        data.VIT = CharData.VIT;
                        data.AGI = CharData.AGI;
                        data.INT = CharData.INT;
                        data.LUK = CharData.LUK;
                    data.EXP = CharData.EXP;
                        db.SaveChanges();
                        check_bool = true;
                    
                    if (!check_bool)
                    {
                        db.CharacterData.AddRange(character.CharData);
                        db.SaveChanges();
                    }
                }
            }

        }

        public int CountDamage(int ATK, int DEF)
        {
            int Damage = 0;
            if (ATK - DEF <= 0)
            {
                goto rezero;
            }
            else
            {
                Damage = ATK - DEF;
            }
        rezero:
            return Damage;
        }
    }

    public abstract class JobLevelUp
    {
        public abstract void LevelUp(int HPUP, int MPUP, int ATKUP, int DEFUP, int STRUP, int VITUP, int AGIUP, int INTUP, int LUKUP, CharacterData CharData);
    }
    public class SwordMan : JobLevelUp
    {
        public override void LevelUp(int HPUP, int MPUP, int ATKUP, int DEFUP, int STRUP, int VITUP, int AGIUP, int INTUP, int LUKUP, CharacterData CharData)
        {
            HPUP = (CharData.VIT * 2) + 20;
            MPUP = CharData.INT;
            STRUP = CharData.LV * 5 + 1;
            VITUP = CharData.LV * 4;
            AGIUP = CharData.LV * 2;
            INTUP = CharData.LV;
            LUKUP = CharData.LV * 2;
            ATKUP = STRUP;
            DEFUP = VITUP;

            CharData.LV++;
            CharData.HP += HPUP;
            CharData.MP += MPUP;
            CharData.STR += STRUP;
            CharData.VIT += VITUP;
            CharData.AGI += AGIUP;
            CharData.INT += INTUP;
            CharData.LUK += LUKUP;
            CharData.ATK += ATKUP;
            CharData.DEF += DEFUP;
            Console.WriteLine("\n您的角色升級了! 血量增加了 {0}\t魔力增加了 {1}\n攻擊增加了 {2}\t防禦增加了 {3}\t力量增加了 {4}\t體質增加了 {5}\t敏捷增加了 {6}\t智力增加了 {7}\t幸運增加了 {8}",
                                                HPUP, MPUP, ATKUP, DEFUP, STRUP, VITUP, AGIUP, INTUP, LUKUP);
        }
    }
    public class Mage : JobLevelUp
    {
        public override void LevelUp(int HPUP, int MPUP, int ATKUP, int DEFUP, int STRUP, int VITUP, int AGIUP, int INTUP, int LUKUP, CharacterData CharData)
        {
            HPUP = (CharData.VIT * 2);
            MPUP = CharData.INT + 20;
            STRUP = CharData.LV;
            VITUP = CharData.LV * 2;
            AGIUP = CharData.LV * 2;
            INTUP = CharData.LV * 5 + 1;
            LUKUP = CharData.LV * 4;
            ATKUP = STRUP;
            DEFUP = VITUP;

            CharData.LV++;
            CharData.HP += HPUP;
            CharData.MP += MPUP;
            CharData.STR += STRUP;
            CharData.VIT += VITUP;
            CharData.AGI += AGIUP;
            CharData.INT += INTUP;
            CharData.LUK += LUKUP;
            CharData.ATK += ATKUP;
            CharData.DEF += DEFUP;
            Console.WriteLine("\n您的角色升級了! 血量增加了 {0}\t魔力增加了 {1}\n攻擊增加了 {2}\t防禦增加了 {3}\t力量增加了 {4}\t體質增加了 {5}\t敏捷增加了 {6}\t智力增加了 {7}\t幸運增加了 {8}",
                                                HPUP, MPUP, ATKUP, DEFUP, STRUP, VITUP, AGIUP, INTUP, LUKUP);
        }
    }
    public class Archer : JobLevelUp
    {
        public override void LevelUp(int HPUP, int MPUP, int ATKUP, int DEFUP, int STRUP, int VITUP, int AGIUP, int INTUP, int LUKUP, CharacterData CharData)
        {
            HPUP = (CharData.VIT * 2);
            MPUP = CharData.INT;
            STRUP = CharData.LV * 4;
            VITUP = CharData.LV * 2;
            AGIUP = CharData.LV * 5;
            INTUP = CharData.LV;
            LUKUP = CharData.LV * 2;
            ATKUP = STRUP;
            DEFUP = VITUP;

            CharData.LV++;
            CharData.HP += HPUP;
            CharData.MP += MPUP;
            CharData.STR += STRUP;
            CharData.VIT += VITUP;
            CharData.AGI += AGIUP;
            CharData.INT += INTUP;
            CharData.LUK += LUKUP;
            CharData.ATK += ATKUP;
            CharData.DEF += DEFUP;
            Console.WriteLine("\n您的角色升級了! 血量增加了 {0}\t魔力增加了 {1}\n攻擊增加了 {2}\t防禦增加了 {3}\t力量增加了 {4}\t體質增加了 {5}\t敏捷增加了 {6}\t智力增加了 {7}\t幸運增加了 {8}",
                                                HPUP, MPUP, ATKUP, DEFUP, STRUP, VITUP, AGIUP, INTUP, LUKUP);
        }
    }
    public class Assassin : JobLevelUp
    {
        public override void LevelUp(int HPUP, int MPUP, int ATKUP, int DEFUP, int STRUP, int VITUP, int AGIUP, int INTUP, int LUKUP, CharacterData CharData)
        {
            HPUP = (CharData.VIT * 2);
            MPUP = CharData.INT;
            STRUP = CharData.LV * 2;
            VITUP = CharData.LV * 2;
            AGIUP = CharData.LV * 4;
            INTUP = CharData.LV;
            LUKUP = CharData.LV * 5;
            ATKUP = STRUP;
            DEFUP = VITUP;

            CharData.LV++;
            CharData.HP += HPUP;
            CharData.MP += MPUP;
            CharData.STR += STRUP;
            CharData.VIT += VITUP;
            CharData.AGI += AGIUP;
            CharData.INT += INTUP;
            CharData.LUK += LUKUP;
            CharData.ATK += ATKUP;
            CharData.DEF += DEFUP;
            Console.WriteLine("\n您的角色升級了! 血量增加了 {0}\t魔力增加了 {1}\n攻擊增加了 {2}\t防禦增加了 {3}\t力量增加了 {4}\t體質增加了 {5}\t敏捷增加了 {6}\t智力增加了 {7}\t幸運增加了 {8}",
                                                HPUP, MPUP, ATKUP, DEFUP, STRUP, VITUP, AGIUP, INTUP, LUKUP);
        }
    }
    public class Monk : JobLevelUp
    {
        public override void LevelUp(int HPUP, int MPUP, int ATKUP, int DEFUP, int STRUP, int VITUP, int AGIUP, int INTUP, int LUKUP, CharacterData CharData)
        {
            HPUP = (CharData.VIT * 2);
            MPUP = CharData.INT;
            STRUP = CharData.LV;
            VITUP = CharData.LV * 2;
            AGIUP = CharData.LV * 2;
            INTUP = CharData.LV * 5;
            LUKUP = CharData.LV * 4;
            ATKUP = STRUP;
            DEFUP = VITUP;

            CharData.LV++;
            CharData.HP += HPUP;
            CharData.MP += MPUP;
            CharData.STR += STRUP;
            CharData.VIT += VITUP;
            CharData.AGI += AGIUP;
            CharData.INT += INTUP;
            CharData.LUK += LUKUP;
            CharData.ATK += ATKUP;
            CharData.DEF += DEFUP;
            Console.WriteLine("\n您的角色升級了! 血量增加了 {0}\t魔力增加了 {1}\n攻擊增加了 {2}\t防禦增加了 {3}\t力量增加了 {4}\t體質增加了 {5}\t敏捷增加了 {6}\t智力增加了 {7}\t幸運增加了 {8}",
                                                HPUP, MPUP, ATKUP, DEFUP, STRUP, VITUP, AGIUP, INTUP, LUKUP);
        }
    }
}