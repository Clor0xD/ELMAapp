using ELMAapp.DAL;
using ELMAapp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Security;

namespace ELMAapp.Service
{
    public class DocumentService
    {
        /// <summary>
        /// Пересоздавать фабрику сессий накладно
        /// </summary>
        private static ConcurrentDictionary<string, DocRepository> docRepositories =
            new ConcurrentDictionary<string, DocRepository>();

        private DocRepository docRepository;

        public DocumentService()
        {
            var currentUser = Membership.GetUser();
            if (string.IsNullOrEmpty(currentUser?.UserName))
            {
                throw new Exception("Authorization Error");
            }

            docRepository = docRepositories.GetOrAdd(currentUser.UserName, (key) => new DocRepository());
        }

        private static DocumentsViewModel ToModel(Documents doc)
        {
            return new DocumentsViewModel()
            {
                Document = doc,
                Date = doc.Date.ToShortDateString() + " " + doc.Date.ToShortTimeString(),
                BinaryFileLimit = doc.BinaryFile.Length > 30
                    ? doc.BinaryFile.Substring(0, 30) + "..."
                    : doc.BinaryFile
            };
        }

        public IEnumerable<DocumentsViewModel> SearchAndSortDocuments(bool reverse, string sortBy, SearchModel search)
        {
            var documents = docRepository.SearchAndSortDocuments(
                reverse, sortBy, search.SelectField, search.SearchString, search.StartDate, search.EndDate.AddDays(1));
            return documents.Select(ToModel).ToList();
        }

        public int CreateDocument(CreateDocModel createDocModel)
        {
            return docRepository.CreateDocument(createDocModel);
        }

        public Documents GetDocumentByID(int id)
        {
            return docRepository.GetDocumentByID(id);
        }
    }
}