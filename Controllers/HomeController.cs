using ExpenceTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace ExpenceTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DBContext db;

        public HomeController(ILogger<HomeController> logger,DBContext _db)
        {
            _logger = logger;
            db=_db;
        }
        [Authorize]
        public IActionResult Index()

        {
          
            var cat = db.Accountscatagories.ToList();
            var data = db.Accountsheads.ToList();
            List<HeadModel> hd=new List<HeadModel>();
            foreach (var i in cat)
            {
                hd.Add(new HeadModel { accid = i.catid,code=i.code,headname=i.catagory,ishead=true });
            }
            foreach (var i in data)
            {
                hd.Add(new HeadModel { accid = i.catid, code = i.code, headname = i.expence, ishead = false });
            }
            return View(hd.OrderBy(o=> o.code));
        }
        [HttpPost]
        public IActionResult Expensehead(Accountshead ac)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ac.code = Getcode(ac.catid);
                    db.Accountsheads.Add(ac);
                    db.SaveChanges();
                 
                }
                catch 
                {

                    TempData["msg"] = "Accounts name exist";
                }
            }
            return RedirectToAction("Index");
        }
        public long Getcode(int catid)
        {
            long code = 0; 
           var check= db.Accountsheads.Where(w=> w.catid==catid).OrderByDescending(o=> o.accid).FirstOrDefault();
            if (check==null)
            {
                code=db.Accountscatagories.Where(w=>w.catid==catid).First().code+101;
            }
            else
            {
                code=check.code+1;
            }
            return code;
        }
        [Authorize]
        public IActionResult Cash()
        {
            var cash = db.Accountsheads.Where(w => w.cat.iscapital == true).ToList();
            return View(cash);
        }
        [HttpPost]
        public IActionResult Addcash(Transections trs)
        { 
            List<Models.Transaction> trx = new List<Models.Transaction>();
            trx.Add(new Models.Transaction {accid=trs.accid,debit=trs.amount });
           var commit = Transactionbalance(trx);
            if (commit==true)
            {
                return RedirectToAction("Cash");
            }
            else
            {
                TempData["msg"] = "Something is wrong";
                return RedirectToAction("Cash");
            }
            
        }
        [Authorize]
        public IActionResult Expense()
        {
            var cash = db.Accountsheads.Where(w => w.cat.iscapital== true).ToList();
            var lgr = db.Accountsheads.Where(w => w.cat.iscapital == false).ToList();
            Headbind hb =new Headbind();
            hb.cash = cash;
            hb.exp = lgr;
            return View(hb);
        }
        [HttpPost]
        public IActionResult Addexpense(Transections trs)
        {
            List<Models.Transaction> trx = new List<Models.Transaction>();
            trx.Add(new Models.Transaction { accid = trs.cashid, credit = trs.amount });
            trx.Add(new Models.Transaction { accid = trs.accid, debit = trs.amount });
            var commit = Transactionbalance(trx);
            if (commit == true)
            {
                return RedirectToAction("Expense");
            }
            else
            {
                TempData["msg"] = "Something is wrong";
                return RedirectToAction("Expense");
            }
        }
        [Authorize]
        public IActionResult Dailyrtx(DateTime? dtt)
        
        { 
            DateTime dt = dtt == null ? DateTime.Now:(DateTime)dtt;
            dt = DateTime.Parse(dt.ToString("MM/dd/yyyy"));
            decimal totaldebit = 0;
           
            List<Models.Transaction> trx = new List<Models.Transaction>();
           
            var prebalance = (from lc in db.Accountsheads.Where(w=> w.cat.iscapital==true)
                              join t in db.Transactions.Where(w => w.trxdate < dt) on lc.accid equals t.accid into tt
                              from tr in tt.DefaultIfEmpty()
                              group new {  trnid = tr == null ? 0 : tr.trnid } by new { balance = tr == null ? 0 : tr.balance, accid = tr == null ? 0 : tr.accid } into g
                              select new { g.Key.accid, trnid = g.Max(m=> m.trnid),g.Key.balance }).FirstOrDefault();


            trx.Add(new Models.Transaction { remarks = "Previous balance", credit = prebalance.balance });
            var crDiposit = (from lc in db.Accountsheads.Where(w => w.cat.iscapital == true)
                              join t in db.Transactions.Where(w => w.trxdate.Day == dt.Day && w.trxdate.Month == dt.Month && w.trxdate.Year == dt.Year) on lc.accid equals t.accid into tt
                              from tr in tt.DefaultIfEmpty()
                              group new { balance = tr == null ? 0 : tr.balance } by new {  accid = tr == null ? 0 : tr.accid } into g
                              select new { g.Key.accid, balance = g.Sum(m => m.balance) }).FirstOrDefault();


            trx.Add(new Models.Transaction { remarks = "Today diposit", debit = crDiposit.balance });
            var crbalance = (from lc in db.Accountsheads.Where(w => w.cat.iscapital == false)
                             join t in (db.Transactions.Where(w =>  w.trxdate.Day == dt.Day && w.trxdate.Month == dt.Month && w.trxdate.Year == dt.Year)) on lc.accid equals t.accid
                         select new { lc.accid, t.trxno, t.trxdate, t.remarks, t.debit, t.credit }).ToList();
            if (crbalance.Count > 0)
            {
                foreach (var i in crbalance)
                {
                    trx.Add(new Models.Transaction { trxno = i.trxno, trxdate = i.trxdate, remarks = i.remarks, credit = i.debit});
                   
                }
                foreach (var j in crbalance)
                {
                   totaldebit += j.debit;
                 
                }
               
            }
            trx.Add(new Models.Transaction { remarks = "Total expense", credit = totaldebit });
            trx.Add(new Models.Transaction { remarks = "Balance", credit = prebalance.balance - totaldebit+crDiposit.balance });
            return View(trx);
        }
        public bool Transactionbalance(List<Models.Transaction> trs)
        {
            bool s = false;
            using (var transaction =db.Database.BeginTransaction())
            {
               
                try
                {
                    var code= Trxcode();
                    var tdate = DateTime.Now;
                    foreach (var i in trs)
                    {
                        decimal balance = db.Transactions.Where(w => w.accid == i.accid).OrderBy(o => o.trxdate).Select(s=> s.balance).FirstOrDefault();
                     
                        i.trxno = code;
                        i.trxdate=tdate;
                       
                        if (db.Accountsheads.Where(w=> w.accid==i.accid).Select(w=> w.cat.code).FirstOrDefault()== (long)1000)
                        {
                            i.balance = balance + i.debit - i.credit;
                        }
                        else
                        {
                            i.balance = balance - i.debit + i.credit;
                        }
                        db.Transactions.Add(i);
                        db.SaveChanges();
                    }
                   
                    transaction.Commit();
                    s= true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return s;
                }
            }
            return s;
        }
        public long Trxcode()
        {
            long code = 0;
            
            var check=db.Transactions.OrderByDescending(o=> o.trnid).FirstOrDefault();
            if (check==null)
            {
                code = 100000001;
            }
            else
            {
                code +=1;
            }
            return code;
        }

    }
}
