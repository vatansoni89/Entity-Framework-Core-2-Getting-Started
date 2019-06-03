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
        static void Main(string[] args)
        {
            InsertSamurai();
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
