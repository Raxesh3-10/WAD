using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using SAS.Services;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAS.Controllers
{
    public class BillsController : Controller
    {
        private readonly IBillRepository _billRepo;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinary;

        public BillsController(IBillRepository billRepo, IMapper mapper, CloudinaryService cloudinary)
        {
            _billRepo = billRepo;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BillViewModel billVm)
        {
            if (!IsAuthorized("staff")) return Unauthorized();

            var bill = _mapper.Map<Bill>(billVm);
            bill.Id = Guid.NewGuid();

            bill.VendorName = HttpContext.Session.GetString("UserName") ?? "Unknown";
            bill.VendorEmail = HttpContext.Session.GetString("UserEmail") ?? "unknown@example.com";

            var docUrls = new List<string>();

            if (billVm.NewDocuments != null && billVm.NewDocuments.Any())
            {
                foreach (var file in billVm.NewDocuments)
                {
                    var url = _cloudinary.UploadFile(file, "documents");
                    if (!string.IsNullOrEmpty(url))
                        docUrls.Add(url);
                }
            }

            bill.Documents = string.Join(";", docUrls);

            _billRepo.Add(bill);

            return RedirectToRoleDashboard();
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            if (!IsAuthorized("staff", "principal")) return HandleUnauthorized();

            var bill = _billRepo.GetById(id);
            if (bill == null) return NotFound();

            var vm = _mapper.Map<BillViewModel>(bill);
            vm.Documents = bill.Documents ?? string.Empty;

            return PartialView("_EditBill", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, BillViewModel vm)
        {
            if (!IsAuthorized("staff", "principal")) return HandleUnauthorized();
            if (id != vm.Id) return NotFound();

            var bill = _billRepo.GetById(id);
            if (bill == null) return NotFound();

            var currentDocs = (bill.Documents ?? string.Empty)
                                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                                .ToList();

            _mapper.Map(vm, bill);
            bill.Documents = string.Join(";", currentDocs);

            bill.VendorName = HttpContext.Session.GetString("UserName") ?? "Unknown";
            bill.VendorEmail = HttpContext.Session.GetString("UserEmail") ?? "unknown@example.com";

            if (vm.RemoveDocIndexes != null && vm.RemoveDocIndexes.Any())
            {
                foreach (var index in vm.RemoveDocIndexes.OrderByDescending(x => x))
                {
                    if (index >= 0 && index < currentDocs.Count)
                    {
                        _cloudinary.DeleteFromCloudinary(currentDocs[index], false);
                        currentDocs.RemoveAt(index);
                    }
                }
            }

            if (vm.NewDocuments != null && vm.NewDocuments.Any())
            {
                foreach (var file in vm.NewDocuments)
                {
                    var url = _cloudinary.UploadFile(file, "documents");
                    if (!string.IsNullOrEmpty(url))
                        currentDocs.Add(url);
                }
            }

            bill.Documents = string.Join(";", currentDocs);

            _billRepo.Update(bill);

            return RedirectToRoleDashboard();
        }

        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            if (!IsAuthorized("staff", "principal")) return HandleUnauthorized();

            var bill = _billRepo.GetById(id);
            if (bill != null && !string.IsNullOrEmpty(bill.Documents))
            {
                var docs = bill.Documents.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var doc in docs)
                {
                    _cloudinary.DeleteFromCloudinary(doc, false);
                }
            }

            _billRepo.Delete(id);
            return RedirectToRoleDashboard();
        }

        private bool IsAuthorized(params string[] allowedRoles)
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role != null && allowedRoles.Contains(role);
        }

        private IActionResult HandleUnauthorized()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }

        private IActionResult RedirectToRoleDashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role switch
            {
                "staff" => RedirectToAction("Dashboard", "Staff"),
                "principal" => RedirectToAction("Dashboard", "Principal"),
                _ => HandleUnauthorized()
            };
        }
    }
}