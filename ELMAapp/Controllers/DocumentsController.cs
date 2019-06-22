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
        //private AppDBContext db = CloudContext.DB //приведет к зависанию приложения если Documents контроллер по умолчанию, вот такой баг. Я не нашел как инициализировать это так чтоб не зависало. И да я принципиально хочу этот контроллер в пути по умолчанию.
        //
        // GET: /Documents/

        public ActionResult Index(SearchModel search, bool reverse = false, string sortBy = "Name", string prevSort = "Name")
        {
            if (sortBy != prevSort) reverse = false;
            ControllerContext.RouteData.Values.Add("reverse", reverse);
            ControllerContext.RouteData.Values.Add("sortBy", sortBy);
            if (search == null) search = new SearchModel();
            return View(new ViewModel(ViewDoc.GetViewListDoc(CloudContext.DB, reverse, sortBy, search.SelectCategory, search.SearchString, search.StartDate, search.EndDate.AddDays(1)), search));
        }

        //
        // GET: /Documents/Details/5

        public ActionResult Details(int id = 0)
        {
            Document document = CloudContext.DB.Documents.Find(id);
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
            return View();
        }

        //
        // POST: /Documents/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateDocModel createDocModel)
        {
            if (ModelState.IsValid)
            {
                /*db.Documents.Add(createDocModel);
                db.SaveChanges();*/
                var fileName = Path.GetFileName(createDocModel.BinaryFile.FileName);

                //db.Database.ExecuteSqlCommand("docInsert @p0, @p1",
                //    parameters: new object[] {createDocModel.Name, fileName});

                createDocModel.BinaryFile.SaveAs(Server.MapPath("/Files/" +
                                                                CloudContext.DB.Database.SqlQuery<int>("docInsert @p0, @p1",
                                                                    parameters: new object[]
                                                                        {createDocModel.Name, fileName}).Last() +
                                                                fileName));
                CloudContext.DB.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(createDocModel);
        }

        //
        // GET: /Documents/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Document document = CloudContext.DB.Documents.Find(id);
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
            if (ModelState.IsValid)
            {
                CloudContext.DB.Entry(document).State = EntityState.Modified;
                CloudContext.DB.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(document);
        }

        //
        // GET: /Documents/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Document document = CloudContext.DB.Documents.Find(id);
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
            Document document = CloudContext.DB.Documents.Find(id);
            Debug.Assert(document != null, nameof(document) + " != null");
            CloudContext.DB.Documents.Remove(document);
            CloudContext.DB.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Download(int id = 0)
        {
            var document = CloudContext.DB.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }

            var fileName = document.BinaryFile;
            var fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("/Files" + "/" + document.ID + fileName));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}