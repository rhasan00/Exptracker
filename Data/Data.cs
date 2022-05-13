using ExpenceTracker.Models;
using System.Linq;

namespace ExpenceTracker.Data
{
    public static class DbInitializer
    {
        public static void Initialize(DBContext db)
        {
            db.Database.EnsureCreated();

            if (db.Accountscatagories.ToList().Count == 0)
            {
                var data = new Accountscatagory[]
                      {
                        new Accountscatagory{catagory="Asset(Capital)",code=1000,iscapital=true},
                        new Accountscatagory{catagory="Finincial expence",code=4000,iscapital=false},
                        new Accountscatagory{catagory="Operating expence",code=5000,iscapital=false},

                      };
                foreach (var c in data)
                {
                    db.Accountscatagories.Add(c);
                }
                db.SaveChanges();
            }
            if (db.Accountsheads.ToList().Count == 0)
            {
                var data = new Accountshead[]
                      {
                        new Accountshead{expence="Cash",code=1100,catid=1}
                      };
                foreach (var c in data)
                {
                    db.Accountsheads.Add(c);
                }
                db.SaveChanges();
            }
            if (db.Users.ToList().Count == 0)
            {
                var data = new User[]
                      {
                        new User{username="admin",fullname="Rakib Hasan",role="admin",password="a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"}
                      };
                foreach (var c in data)
                {
                    db.Users.Add(c);
                }
                db.SaveChanges();
            }


        }
    }
}
