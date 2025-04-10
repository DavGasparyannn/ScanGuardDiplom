﻿using ScanGuard.BLL.Interfaces;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.Domain.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ScanGuard.Controllers;

public class FileScanController : Controller
{
    private readonly IFileScanService _scanService;
    private const int DailyScanLimit = 2;
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> SignInManager;
    private readonly UserManager<ApplicationUser> UserManager;


    public FileScanController(IFileScanService scanService, ApplicationDbContext applicationDbContext, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _scanService = scanService ?? throw new ArgumentNullException(nameof(scanService));
        _context = applicationDbContext;
        SignInManager = signInManager;
        UserManager = userManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Пожалуйста, выберите файл");
            return View("Index");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("", "Пользователь не аутентифицирован");
            return View("Index");
        }

        // Проверяем роль пользователя в базе
        var user = await UserManager.GetUserAsync(User);
        user.ScannedFileCount++;
        await _context.SaveChangesAsync();
        bool isCustomer = await UserManager.IsInRoleAsync(user, SD.Role_Customer);

        if (isCustomer)
        {
            // Получаем последнюю проверку пользователя
            var lastScan = await _context.FileScanEntities
                .Where(x => x.ApplicationUserId == userId)
                .OrderByDescending(x => x.ScanDate)
                .FirstOrDefaultAsync();

            if (lastScan != null)
            {
                var timeSinceLastScan = DateTime.Now - lastScan.ScanDate;
                if (timeSinceLastScan.TotalHours < 24)
                {
                    TempData["Notification"] = "Вы можете проверять файлы 1 раз в 24 часа. Получите премиум для безлимитного доступа.";
                    return RedirectToAction("Index");
                }
                else
                {
                    try
                    {
                        var result = await _scanService.ScanFileAsync(file, userId);
                        return RedirectToAction("Result", new { id = result.Id });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Ошибка при сканировании: {ex.Message}");
                        return View("Index");
                    }
                }
            }
            else
            {
                try
                {
                    var result = await _scanService.ScanFileAsync(file, userId);
                    return RedirectToAction("Result", new { id = result.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ошибка при сканировании: {ex.Message}");
                    return View("Index");
                }
            }
        }
        else
        {
            try
            {
                var result = await _scanService.ScanFileAsync(file, userId);
                return RedirectToAction("Result", new { id = result.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при сканировании: {ex.Message}");
                return View("Index");
            }

        }
    }

    [HttpGet]
    public async Task<IActionResult> Result(string id)
    {
        var scanResult = await _scanService.GetScanResultAsync(id);
        if (scanResult == null)
            return NotFound();
        return View(scanResult);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> History()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var history = await _scanService.GetUserScanHistoryAsync(userId);
        return View(history);
    }
}