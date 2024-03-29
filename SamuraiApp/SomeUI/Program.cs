﻿using System;
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
            ModifyingRelatedDataWhenNotTracked();
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
