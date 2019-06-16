using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SamuraiApp.Data;
using SamuraiApp.Domain;

namespace SomeUI
{
    class Program
    {
        public static SamuraiContext Context { get; set; } = new SamuraiContext();

        static void Main(string[] args)
        {
            //InsertSamurai();
           // InsertMultipleSamurai();

           // InsertSumuraiAndBattleAtOneShot();

           // QueryAndUpdateBattle_Disconnected();

           // DeletemanySamurai();
        }

        private static void DeletemanySamurai()
        {
            var samuS = Context.Samurais.Where(s => s.Name.Contains("samu"));

            Context.Samurais.RemoveRange(samuS);
            Context.SaveChanges();
        }

        private static void QueryAndUpdateBattle_Disconnected()
        {
            var battle = Context.Battles.FirstOrDefault();
            battle.EndDate = DateTime.Now.AddYears(-20);

            using (var newContext = new SamuraiContext())
            {
                newContext.Battles.Update(battle);
                newContext.SaveChanges();
            }

        }

        private static void InsertSumuraiAndBattleAtOneShot()
        {
            var samu = new Samurai { Name = "samu1" };
            var battle = new Battle { Name="Panipat",StartDate=DateTime.Now.AddYears(-100), EndDate = DateTime.Now.AddYears(-50) };

            using (var context = new SamuraiContext())
            {
                //added in ef core 2
                context.AddRange(samu, battle);
                context.SaveChanges();
            }
        }

        private static void InsertMultipleSamurai()
        {
            var samurai1 = new Samurai { Name = "samu1" };
            var samurai2 = new Samurai { Name = "samu2" };

            using (var context = new SamuraiContext())
            {
                context.Samurais.AddRange(samurai1,samurai2);
                context.SaveChanges();
            }
        }

        private static void InsertSamurai()
        {
            var samurai = new Samurai { Name = "Julie"};

            using (var context = new SamuraiContext())
            {
                context.Samurais.Add(new Samurai { Name = "Hiii" });
                context.SaveChanges();
            }
        }
    }
}
