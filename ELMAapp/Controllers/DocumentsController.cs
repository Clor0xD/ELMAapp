using System.IO;
using ELMAapp.Models;
using System.Web.Mvc;
using ELMAapp.DAL;
using ELMAapp.Service;

namespace ELMAapp.Controllers
{
    public class DocumentsController : Controller
    {
        private DocumentService documentService;

        public DocumentsController()
        {
            documentService = new DocumentService();
        }

        public ActionResult Index(SearchModel search, bool reverse = false, string sortBy = "Name", string prevSort = "Name")
        {
            if (sortBy != prevSort) reverse = false;
            ControllerContext.RouteData.Values.Add("reverse", reverse);
            ControllerContext.RouteData.Values.Add("sortBy", sortBy);
            if (search == null) search = new SearchModel();
            var documents = documentService.SearchAndSortDocuments(reverse, sortBy, search);
            return View(new DocumentsAndSearchModel(){ViewDocs = documents, Search = search });
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
                    Server.MapPath("/Files/" + documentService.CreateDocument(createDocModel) + fileName));
                return RedirectToAction("Index");
            }

            return View(createDocModel);
        }

        public ActionResult Download(int id = 0)
        {
            var document = documentService.GetDocumentByID(id);
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