using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SamuraiApp.Data;
using SamuraiApp.Domain;

//Imp.1: Include method always loads the entire set of related objects (full graph, not just req. properties)

namespace SomeUI
{
    class Program
    {
        public static SamuraiContext Context { get; set; } = new SamuraiContext();
        public static SamuraiContext _context { get; set; } = new SamuraiContext();

        static void Main(string[] args)
        {
            //InsertSamurai();
            // InsertMultipleSamurai();

            // InsertSumuraiAndBattleAtOneShot();

            // QueryAndUpdateBattle_Disconnected();

            // DeletemanySamurai();

            //InsertNewPkFkgraph();

            //AddChildToExistingObjectWhileNotTracked(1);

            //EagerLoadSamuraiWithQuotes();

            //ProjectSomeProperties();

            //var dynamicList = ProjectDynamic();

            //ProjectSamuraisWithQuotes();

            //ProjectSamuraisWithQuotesHavingInternalFilter();

            //FilteringWithRelatedData();

            //-----Modifying Related Data------
            //ModifyingRelatedDataWhenTracked();
            //ModifyingRelatedDataWhenNotTracked();

            //-----------EF Core 2 Mappings-----------
            //PrePopulateSamuraisAndBattles();
            //JoinBattleAndSamurai();

            //EnlistSamuraiIntoABattle();

            //EnlistSamuraiIntoABattleUntracked();

            //AddNewSamuraiViaDisconnectedBattleObject();

            //GetSamuraiWithBattles();

            //RemoveBattleFromSamurai();

            RemoveBattleFromSamuraiWhenDisconnected();
        }

        /// <summary>
        /// Remove join between Samurai and battle (by using Ids) in disconnected mode.
        /// </summary>
        private static void RemoveBattleFromSamuraiWhenDisconnected()
        {
            Samurai samurai;
            using (var separateOperation = new SamuraiContext())
            {
                samurai = separateOperation.Samurais.Include(s => s.SamuraiBattle)
                    .ThenInclude(sb => sb.Battle)
                    .SingleOrDefault(s => s.Id == 2);
            }

            var sbToRemove = samurai.SamuraiBattle.SingleOrDefault(sb => sb.BattleId == 1);
            samurai.SamuraiBattle.Remove(sbToRemove);

            //_context.Attach(samurai);
            //_context.ChangeTracker.DetectChanges(); //for debugging info _context.ChangeTracker.Entries()

            _context.Remove(sbToRemove);
            _context.ChangeTracker.DetectChanges();
            _context.SaveChanges();
        }

        /// <summary>
        /// Remove join between Samurai and battle (by using Ids) in connected mode.
        /// </summary>
        private static void RemoveBattleFromSamurai()
        {
            //Remove join btw Samurai(Id =1) and  Battle (Id = 3)
            var samurai = _context.Samurais.Include(s => s.SamuraiBattle)
                .ThenInclude(sb => sb.Battle)
                .SingleOrDefault(s => s.Id == 1);

            var sbToRemove = samurai.SamuraiBattle.SingleOrDefault(sb=>sb.BattleId ==3);
            samurai.SamuraiBattle.Remove(sbToRemove); //Ef core knows this need to removed frm DB.

            _context.ChangeTracker.DetectChanges(); //for debugging info _context.ChangeTracker.Entries()
            _context.SaveChanges();
        }

        /// <summary>
        /// If we want to retrive a Samurai and all of the battle Samurai has fought in, 
        /// we would query the Samurai, and then Include the SamuraiBattle attached to the Samurai 
        /// and then Include the Battle that's attached to each one of those SamuraiBattles.
        /// Samurai <Include> SamuraiBattle <ThenInclude> Battle
        /// Note: sb.Battle will not come with intellisence, VS 2017 issue.
        /// </summary>
        private static void GetSamuraiWithBattles()
        {
            var samuraiWithBattle = _context.Samurais.Include(s => s.SamuraiBattle)
                .ThenInclude(sb => sb.Battle).FirstOrDefault();

            var battle = samuraiWithBattle.SamuraiBattle.First().Battle;

            var allTheBattles = new List<Battle>();
            foreach (var samuraiBattle in samuraiWithBattle.SamuraiBattle)
            {
                allTheBattles.Add(samuraiBattle.Battle);
            }

        }


        /// <summary>
        /// Adding Many-to-many Ends on the Fly. 
        /// Add new Samurai to existing battle with many-to-many relationship.
        /// </summary>
        private static void AddNewSamuraiViaDisconnectedBattleObject()
        {
            Battle battle;
            using (var separateOperation = new SamuraiContext())
            {
                battle = separateOperation.Battles.Find(1);
            }
            var newSamurai = new Samurai { Name = "SampsonSan" };
            battle.SamuraiBattle.Add(new SamuraiBattle { Samurai = newSamurai});
            _context.Battles.Attach(battle);
            _context.SaveChanges();
        }

        /// <summary>
        /// Here we use 'Attach(obj)' method instead of Add() bcz in disconnected scenario, 
        /// it will add both Battle and SamuraiBattle as its not tracking them. 
        /// Attach() will only add the obj which don't have key value (i.e. Id).
        /// </summary>
        private static void EnlistSamuraiIntoABattleUntracked()
        {
            Battle battle;
            using (var separateOperation = new SamuraiContext())
            {
                battle = separateOperation.Battles.Find(1);
            }
            battle.SamuraiBattle.Add(new SamuraiBattle { SamuraiId = 9});
            _context.Battles.Attach(battle);
            //_context.Battles.Add(battle);

            /* _context.ChangeTracker.Entries() > ResultView > check the Entity and State props.. */
            _context.ChangeTracker.DetectChanges(); //to show debugging info
            _context.SaveChanges();
        }

        /// <summary>
        /// Here context smartly figureout, the BattleId need to be taken from battle object and 
        /// SamuraiId from the SamuraiBattle object
        /// 
        /// </summary>
        private static void EnlistSamuraiIntoABattle()
        {
            var battle = _context.Battles.Find(1);

            battle.SamuraiBattle.Add(new SamuraiBattle { SamuraiId = 2 });
            _context.SaveChanges();
        }

        /// <summary>
        /// We don't have SamuraiBattle dbset but that is well connected by FK. So _context will figureout 
        /// how to save that as we can perform db operations(add, update, remove etc.) directly on DbContext object.
        /// </summary>
        private static void JoinBattleAndSamurai()
        {
            //If SamuraiId and BattleId is known then we can directly add them.
            var sbJoin = new SamuraiBattle { SamuraiId = 1, BattleId = 3 };

            /*We don't have SamuraiBattle dbset but that is well connected by FK. So _context will figureout 
            how to save that.*/

            _context.Add(sbJoin);
            _context.SaveChanges();
        }

        private static void PrePopulateSamuraisAndBattles()
        {
            _context.AddRange(
             new Samurai { Name = "Kikuchiyo" },
             new Samurai { Name = "Kambei Shimada" },
             new Samurai { Name = "Shichirōji " },
             new Samurai { Name = "Katsushirō Okamoto" },
             new Samurai { Name = "Heihachi Hayashida" },
             new Samurai { Name = "Kyūzō" },
             new Samurai { Name = "Gorōbei Katayama" }
           );

            _context.Battles.AddRange(
             new Battle { Name = "Battle of Okehazama", StartDate = new DateTime(1560, 05, 01), EndDate = new DateTime(1560, 06, 15) },
             new Battle { Name = "Battle of Shiroyama", StartDate = new DateTime(1877, 9, 24), EndDate = new DateTime(1877, 9, 24) },
             new Battle { Name = "Siege of Osaka", StartDate = new DateTime(1614, 1, 1), EndDate = new DateTime(1615, 12, 31) },
             new Battle { Name = "Boshin War", StartDate = new DateTime(1868, 1, 1), EndDate = new DateTime(1869, 1, 1) }
           );
            _context.SaveChanges();
        }

        /// <summary>
        /// Use Entry() over Update() as it only update the required data.
        /// Update() actually updates all data to the DB even if not required.
        /// </summary>
        private static void ModifyingRelatedDataWhenNotTracked()
        {
            var samurai = Context.Samurais.Include(s => s.Quotes).FirstOrDefault();
            var quote = samurai.Quotes[0];
            quote.Text += " --->I M new Text";

            using (var newContext = new SamuraiContext())
            {
                newContext.Entry(quote).State = EntityState.Modified;
                newContext.SaveChanges();
            }
        }

        private static void ModifyingRelatedDataWhenTracked()
        {
            var samurai = Context.Samurais.Include(s => s.Quotes).FirstOrDefault();
            Context.Quotes.Remove(samurai.Quotes[0]);
            Context.SaveChanges();
        }

        private static void FilteringWithRelatedData()
        {
            //Filter only Samurai whose quote have word 'Happy' ,to have Quots also use Include()
            var samurais = Context.Samurais
                                  .Where(s => s.Quotes.Any(q => q.Text.Contains("happy")))
                                  .ToList();
        }

        private static void ProjectSamuraisWithQuotes()
        {
            var samuWithQuotes = Context.Samurais
                .Select(s => new { Samurai = s, Quotes = s.Quotes, count = s.Quotes.Count }).ToList();
        }

        private static void ProjectSamuraisWithQuotesHavingInternalFilter()
        {
            
            var happySamu = Context.Samurais
                .Select(s => new { Samurai = s, HappyQuotes = s.Quotes.Where(q => q.Text.Contains("Happy")), count = s.Quotes.Count })
                .ToList();
        }

        private static List<dynamic> ProjectDynamic()
        {
            var someProp = Context.Samurais.Select(s => new { s.Id, s.Name }).ToList();
            return someProp.ToList<dynamic>();
        }

        private static void ProjectSomeProperties()
        {
            var someProp = Context.Samurais.Select(s => new { s.Id, s.Name }).ToList();
        }

        //using Microsoft.EntityFrameworkCore; for Include
        /// <summary>
        /// Refer "Slides by julia\querying-and-saving-related-data-slides.pdf"
        /// </summary>
        private static void EagerLoadSamuraiWithQuotes()
        {
            var samuraies = Context.Samurais.Include(s => s.Quotes).ToList();
        }

        //Adding Quote in disconnected scenario 
        /// <summary>
        /// we must take care of 'Foreign Keys', here we used SamuraiId for adding Quote to Samurai using FK.
        /// </summary>
        /// <param name="samuId"></param>
        private static void AddChildToExistingObjectWhileNotTracked(int samuId)
        {
            var quote = new Quote {
                Text = "added quote by AddChildToExistingObjectWhileNotTracked",
                SamuraiId = samuId
            };
            var quote1 = new Quote
            {
                Text = "happy",
                SamuraiId = samuId
            };

            using (var context = new SamuraiContext())
            {
                context.Quotes.AddRange(quote, quote1);
                context.SaveChanges();
            }
        }

        private static void InsertNewPkFkgraph()
        {
            var samurai = new Samurai {
                                        Name = "Pk1", Quotes = new List<Quote>()
                                        { new Quote {Text = "quote1 of Pk1" },
                                          new Quote{Text = "quote2 of Pk1" }  }
                                       };

            Context.Samurais.Add(samurai);
            Context.SaveChanges();
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
