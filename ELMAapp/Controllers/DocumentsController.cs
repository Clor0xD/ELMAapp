using ELMAapp.Models;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ELMAapp.Controllers
{
    public class DocumentsController : Controller
    {
        private AppDBContext db = CloudContext.CreateDbContext();// пробовал иначе, но увы куча багов в библиотеках, EF4 не работает conn state и еще всякие другие вроде зависания приложения при некоторых вариантах аунтификации
        //
        // GET: /Documents/

        public ActionResult Index(SearchModel search, bool reverse = false, string sortBy = "Name", string prevSort = "Name")
        {
            if (db == null)
            {
                ViewBag.Error = "произошла ошибка аутентификации";
                return View("Error");
            }
            if (sortBy != prevSort) reverse = false;
            ControllerContext.RouteData.Values.Add("reverse", reverse);
            ControllerContext.RouteData.Values.Add("sortBy", sortBy);
            if (search == null) search = new SearchModel();
            return View(new ViewModel(ViewDoc.GetViewListDoc(db, reverse, sortBy, search.SelectCategory, search.SearchString, search.StartDate, search.EndDate.AddDays(1)), search)); 
        }

        //
        // GET: /Documents/Details/5

        public ActionResult Details(int id = 0)
        {
            if (db == null)
            {
                ViewBag.Error = "произошла ошибка аутентификации";
                return View("Error");
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }

            return View(document);
        }

        //
        // GET: /Documents/Create

        public ActionResult Create()
        {
            if (db == null)
            {
                ViewBag.Error = "произошла ошибка аутентификации";
                return View("Error");
            }
            return View();
        }

        //
        // POST: /Documents/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateDocModel createDocModel)
        {
            if (db == null)
            {
                ViewBag.Error = "произошла ошибка аутентификации";
                return View("Error");
            }
            if (ModelState.IsValid)
            {
                var fileName = Path.GetFileName(createDocModel.BinaryFile.FileName);
                createDocModel.BinaryFile.SaveAs(Server.MapPath("/Files/" +
                                                                db.Database.SqlQuery<int>("docInsert @p0, @p1",
                                                                    parameters: new object[]
                                                                        {createDocModel.Name, fileName}).Last() +
                                                                fileName));
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(createDocModel);
        }

        //
        // GET: /Documents/Edit/5

        public ActionResult Edit(int id = 0)
        {
            if (db == null)
            {
                ViewBag.Error = "произошла ошибка аутентификации";
                return View("Error");
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }

            return View(document);
        }

        //
        // POST: /Documents/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Document document)
        {
            if (db == null)
            {
                ViewBag.Error = "произошла ошибка аутентификации";
                return View("Error");
            }
            if (ModelState.IsValid)
            {
                db.Entry(document).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(document);
        }

        //
        // GET: /Documents/Delete/5

        public ActionResult Delete(int id = 0)
        {
            if (db == null)
            {
                ViewBag.Error = "произошла ошибка аутентификации";
                return View("Error");
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }

            return View(document);
        }

        //
        // POST: /Documents/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (db == null)
            {
                ViewBag.Error = "произошла ошибка аутентификации";
                return View("Error");
            }
            Document document = db.Documents.Find(id);
            Debug.Assert(document != null, nameof(document) + " != null");
            db.Documents.Remove(document);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Download(int id = 0)
        {
            if (db == null)
            {
                ViewBag.Error = "произошла ошибка аутентификации";
                return View("Error");
            }
            var document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }

            var fileName = document.BinaryFile;
            var fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("/Files" + "/" + document.ID + fileName));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}