using System.IO;
using ELMAapp.Models;
using System.Web.Mvc;

namespace ELMAapp.Controllers
{
    public class DocumentsController : Controller
    {
        private DocRepository docRepos;

        public DocumentsController()
        {
            docRepos = new DocRepository();
        }

        public ActionResult Index(SearchModel search, bool reverse = false, string sortBy = "Name", string prevSort = "Name")
        {
            if (sortBy != prevSort) reverse = false;
            ControllerContext.RouteData.Values.Add("reverse", reverse);
            ControllerContext.RouteData.Values.Add("sortBy", sortBy);
            if (search == null) search = new SearchModel();
            return View(new DocumentsAndSearchModel(
                DocumentsViewModel.GetViewModel(docRepos.SearchAndSortDocuments(reverse, sortBy, search.SelectCategory,
                    search.SearchString, search.StartDate, search.EndDate.AddDays(1))),
                search
            ));
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateDocModel createDocModel)
        {
            if (ModelState.IsValid)
            {
                var fileName = Path.GetFileName(createDocModel.BinaryFile.FileName);
                createDocModel.BinaryFile.SaveAs(
                    Server.MapPath("/Files/" + docRepos.CreateDocument(createDocModel) + fileName));
                return RedirectToAction("Index");
            }

            return View(createDocModel);
        }

        public ActionResult Download(int id = 0)
        {
            var document = docRepos.GetDocumentByID(id);
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