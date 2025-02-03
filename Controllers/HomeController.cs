using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using TaskMonitoringApp.Exceptions;
using TaskMonitoringApp.Models;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;
using TaskMonitoringApp.Models.ViewModel;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGoalServices _servicesGoal;
        private readonly ITaskServices _servicesTask;
        private readonly IMapper _mapper;
        private readonly UserManager<Users> _userManager;

        public HomeController(ILogger<HomeController> logger, IGoalServices servicesGoal, ITaskServices serviceTask, IMapper mapper, UserManager<Users> userManager)
        {
            _logger = logger;
            _servicesGoal = servicesGoal;
            _servicesTask = serviceTask;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var goalProductivity = await _servicesGoal.GetGoalProductivity(userId);
            var taskProductivity = await _servicesTask.GetTaskProductivity(userId);

            double avgGrowthPercentage = ((double)(goalProductivity.GrowthPercentage + taskProductivity.GrowthPercentage) / 2);
            avgGrowthPercentage = Math.Round(avgGrowthPercentage, 2);

            double avgProductivity = ((double)(goalProductivity.Productivity + taskProductivity.Productivity) / 2);
            avgProductivity = Math.Round(avgProductivity, 2);

            DashboardProductivityViewModel productivity = new DashboardProductivityViewModel();

            productivity.CompletedTasks = taskProductivity.CompletedTask;
            productivity.CompletedGoals = goalProductivity.CompletedGoal;
            productivity.GrowthPercentage = avgGrowthPercentage;
            productivity.Productivity = avgProductivity;


            return View(productivity);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
