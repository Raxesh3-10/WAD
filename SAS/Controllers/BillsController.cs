using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAS.Models;
using SAS.Repositories;
using SAS.ViewModels;
using AutoMapper;
using System;
using System.Linq;

namespace SAS.Controllers
{
    public class BillsController : Controller
    {
        private readonly IBillRepository _billRepo;
        private readonly IMapper _mapper;

        public BillsController(IBillRepository billRepo, IMapper mapper)
        {
            _billRepo = billRepo;
            _mapper = mapper;
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

            _billRepo.Add(bill);

            TempData["SuccessMessage"] = "Bill created successfully.";
            return RedirectToRoleDashboard();
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            if (!IsAuthorized("staff" , "principal")) return HandleUnauthorized();

            var bill = _billRepo.GetById(id);
            if (bill == null) return NotFound();

            var vm = _mapper.Map<BillViewModel>(bill);
            return PartialView("_EditBill", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, BillViewModel vm)
        {
            if (!IsAuthorized("staff" ,"principal")) return HandleUnauthorized();
            if (id != vm.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var bill = _mapper.Map<Bill>(vm);

                bill.VendorName = HttpContext.Session.GetString("UserName") ?? "Unknown";
                bill.VendorEmail = HttpContext.Session.GetString("UserEmail") ?? "unknown@example.com";

                _billRepo.Update(bill);

                TempData["SuccessMessage"] = "Bill updated successfully.";
                return RedirectToRoleDashboard();
            }

            return View(vm);
        }

        [HttpPost]
        public IActionResult Delete(Guid id)
        {
            if (!IsAuthorized("staff", "principal")) return HandleUnauthorized();

            _billRepo.Delete(id);
            TempData["SuccessMessage"] = "Bill deleted successfully.";
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