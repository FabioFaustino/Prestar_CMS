using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prestar.Data;
using Prestar.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Controllers
{
    /// <summary>
    /// Statistics Controller
    /// </summary>
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Constructor of the StatisticsController class that receives two parameters 
        /// and initializes them.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="context">
        /// Application database context
        /// <see cref="ApplicationDbContext"/>
        /// </param>
        public StatisticsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets Moderator's Statistics
        /// </summary>
        /// <returns>Returns an IActionResult</returns>
        /// <seealso cref="IActionResult"/>
        [HttpGet, HttpPost]
        [Authorize(Roles = "Administrador, Moderador")]
        public IActionResult Moderador()
        {
            //AverageEvaluationEachProvider
            int providers = CountTheProviders();
            if (providers > 0) { 
                double min;
                double max;
                try {
                    min = Convert.ToDouble(Request.Form["min"].ToString().Replace(".", ","));
                }
                catch (Exception) {
                    min = 0.00;
                }
                try
                {
                    max = Convert.ToDouble(Request.Form["max"].ToString().Replace(".", ","));
                }
                catch (Exception)
                {
                    max = 5.00;
                }
                Dictionary<List<string>, List<double>> averageEvaluationEachProvider = AverageEvaluationEachProvider(min, max);
                List<string> names = new List<string>();
                averageEvaluationEachProvider.ElementAt(0).Key.ForEach(id => {
                    User u = _context.Users.FindAsync(id).Result;
                    names.Add(u.FirstName + " " + u.LastName);
                    });
                ViewBag.AverageEvaluationEachProvider_User = names;
                ViewBag.AverageEvaluationEachProvider_Avg = averageEvaluationEachProvider.ElementAt(0).Value;
                ViewBag.Max = Convert.ToDouble(Math.Round(max, 2));
                ViewBag.Min = Convert.ToDouble(Math.Round(min, 2));
                ViewBag.AverageEvaluationEachProvider = averageEvaluationEachProvider;
            }
            ViewBag.NumberOfProviders = providers;

            //ListofProvidersWithTime
            DateTime updateDate = DateTime.Now.Add(new TimeSpan(0, -1, 0, 0));
            string updateType = "1_Hora";
            if (providers > 0) { 
                try
                {
                    updateType = Request.Form["time"].ToString();
                    switch (Request.Form["time"].ToString())
                    {
                        case "1_Dia":
                            updateDate = DateTime.Now.Add(new TimeSpan(-1, 0, 0, 0));
                            break;
                        case "3_Dias":
                            updateDate = DateTime.Now.Add(new TimeSpan(-3, 0, 0, 0));
                            break;
                        case "1_Semana":
                            updateDate = DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0));
                            break;
                        case "4_Semanas":
                            updateDate = DateTime.Now.Add(new TimeSpan(-28, 0, 0, 0));
                            break;
                        default:
                            updateDate = DateTime.Now.Add(new TimeSpan(0, -1, 0, 0));
                            break;
                    }
                }
                catch (Exception)
                {
                    updateDate = DateTime.Now.Add(new TimeSpan(0, -1, 0, 0));
                }
                ViewBag.TypeTime = updateType;
                ViewBag.ListofProvidersWithTime = ListofProvidersWithTime(updateDate);
            }

            //NumberOfClientsServiceConclued
            int[] conclued = new int[4];
            conclued[0] = NumberOfClientsService(ServiceRequisitionStatus.Concluded, DateTime.Now.Add(new TimeSpan(-1, 0, 0, 0)));
            conclued[1] = NumberOfClientsService(ServiceRequisitionStatus.Concluded, DateTime.Now.Add(new TimeSpan(-3, 0, 0, 0)));
            conclued[2] = NumberOfClientsService(ServiceRequisitionStatus.Concluded, DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0)));
            conclued[3] = NumberOfClientsService(ServiceRequisitionStatus.Concluded, DateTime.Now.Add(new TimeSpan(-28, 0, 0, 0)));
            int[] rejected = new int[4];
            rejected[0] = NumberOfClientsService(ServiceRequisitionStatus.Rejected, DateTime.Now.Add(new TimeSpan(-1, 0, 0, 0)));
            rejected[1] = NumberOfClientsService(ServiceRequisitionStatus.Rejected, DateTime.Now.Add(new TimeSpan(-3, 0, 0, 0)));
            rejected[2] = NumberOfClientsService(ServiceRequisitionStatus.Rejected, DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0)));
            rejected[3] = NumberOfClientsService(ServiceRequisitionStatus.Rejected, DateTime.Now.Add(new TimeSpan(-28, 0, 0, 0)));
            int[] cancelled = new int[4];
            cancelled[0] = NumberOfClientsService(ServiceRequisitionStatus.Cancelled, DateTime.Now.Add(new TimeSpan(-1, 0, 0, 0)));
            cancelled[1] = NumberOfClientsService(ServiceRequisitionStatus.Cancelled, DateTime.Now.Add(new TimeSpan(-3, 0, 0, 0)));
            cancelled[2] = NumberOfClientsService(ServiceRequisitionStatus.Cancelled, DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0)));
            cancelled[3] = NumberOfClientsService(ServiceRequisitionStatus.Cancelled, DateTime.Now.Add(new TimeSpan(-28, 0, 0, 0)));
            ViewBag.NumberOfClientsServiceConclued = conclued;
            ViewBag.NumberOfClientsServiceRejected = rejected;
            ViewBag.NumberOfClientsServiceCancelled = cancelled;
            ViewBag.NumberServiceRequisition = _context.ServiceRequisition.Count();
            ViewBag.TypeDuration = updateType;


            //NumberOfUsersWithoutActivity
            int[] withoutActivity = new int[6];
            withoutActivity[0] = NumberOfUsersWithoutActivity(DateTime.Now.Add(new TimeSpan(-3, 0, 0, 0)));
            withoutActivity[1] = NumberOfUsersWithoutActivity(DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0)));
            withoutActivity[2] = NumberOfUsersWithoutActivity(DateTime.Now.Add(new TimeSpan(-15, 0, 0, 0)));
            withoutActivity[3] = NumberOfUsersWithoutActivity(DateTime.Now.Add(new TimeSpan(-30, 0, 0, 0)));
            withoutActivity[4] = NumberOfUsersWithoutActivity(DateTime.Now.Add(new TimeSpan(-50, 0, 0, 0)));
            withoutActivity[5] = NumberOfUsersWithoutActivity(DateTime.Now.Add(new TimeSpan(-100, 0, 0, 0)));
            ViewBag.NumberOfUsersWithoutActivity = withoutActivity;

            //NumberOfComplaints
            int numberOfComplaints = _context.Complaint.Count();
            if (numberOfComplaints > 0) { 
                int[] complaints = new int[3];
                complaints[0] = NumberOfComplaints(ComplaintType.ReportClient);
                complaints[1] = NumberOfComplaints(ComplaintType.ReportService);
                complaints[2] = NumberOfComplaints(ComplaintType.ReportServiceProvider);
                ViewBag.NumberOfComplaints = complaints;
            }
            else
            {
                ViewBag.NumberOfComplaints = new int[3] {0, 0, 0};
            }
            ViewBag.TotalNumberOfComplaints = numberOfComplaints;

            //Ativities
            List<int> ativities = new List<int>();
            List<int> ativitiesCompleted = new List<int>();
            if (_context.ServiceRequisition.Count() > 0) { 
                ativities.Add(NumberOfRequisition(DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0))));
                ativitiesCompleted.Add(NumberOfRequisitionCompleted(DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0))));
                ativities.Add(NumberOfRequisition(DateTime.Now.Add(new TimeSpan(-14, 0, 0, 0))));
                ativitiesCompleted.Add(NumberOfRequisitionCompleted(DateTime.Now.Add(new TimeSpan(-14, 0, 0, 0))));
                ativities.Add(NumberOfRequisition(DateTime.Now.Add(new TimeSpan(-21, 0, 0, 0))));
                ativitiesCompleted.Add(NumberOfRequisitionCompleted(DateTime.Now.Add(new TimeSpan(-21, 0, 0, 0))));
                ativities.Add(NumberOfRequisition(DateTime.Now.Add(new TimeSpan(-30, 0, 0, 0))));
                ativitiesCompleted.Add(NumberOfRequisitionCompleted(DateTime.Now.Add(new TimeSpan(-30, 0, 0, 0))));
            }
            else
            {
                ativities.AddRange(new List<int> { 0, 0, 0, 0, 0 });
                ativitiesCompleted.AddRange(new List<int> { 0, 0, 0, 0, 0 });
            }
            ViewBag.NumberOfRequisitions = ativities;
            ViewBag.NumberOfRequisitionsCompleted = ativitiesCompleted;

            //AverageAge
            ViewBag.AverageAge = AverageAge();
            ViewBag.Ages = Ages();

            //AverageEnrolls
            double averageEnrolls = 0.0;
            if (_context.Formation.Count() > 1)
            {
                averageEnrolls = AverageEnrolls();
            }
            ViewBag.AverageEnrolls = averageEnrolls;
            Dictionary<List<string>, List<int>> enrolls = NewestEnrolls();
            ViewBag.NewestEnrolls_Title = enrolls.ElementAt(0).Key;
            ViewBag.NewestEnrolls_Number = enrolls.ElementAt(0).Value;

            //Users
            ViewBag.NumberUsers = _context.Users.Count();

            return View();
        }

        /// <summary>
        /// Gets provider's statistics
        /// </summary>
        /// <returns>Returns an IActionResult</returns>
        /// <seealso cref="IActionResult"/>
        [HttpGet, HttpPost]
        [Authorize(Roles = "Prestador")]
        public IActionResult Prestador()
        {
            //Clientes
            int[] clients = new int[5];
            clients[0] = NumberOfClients(DateTime.Now.Add(new TimeSpan(-1, 0, 0, 0))).Result;
            clients[1] = NumberOfClients(DateTime.Now.Add(new TimeSpan(-3, 0, 0, 0))).Result;
            clients[2] = NumberOfClients(DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0))).Result;
            clients[3] = NumberOfClients(DateTime.Now.Add(new TimeSpan(-15, 0, 0, 0))).Result;
            clients[4] = NumberOfClients(DateTime.Now.Add(new TimeSpan(-30, 0, 0, 0))).Result;
            ViewBag.NumberOfClients = clients;

            //Serviços
            int[] services = new int[5];
            services[0] = NumberOfServices(ServiceRequisitionStatus.Cancelled).Result;
            services[1] = NumberOfServices(ServiceRequisitionStatus.Concluded).Result;
            services[2] = NumberOfServices(ServiceRequisitionStatus.Rejected).Result;
            services[3] = NumberOfServices(ServiceRequisitionStatus.Pending).Result;
            services[4] = NumberOfServices(ServiceRequisitionStatus.Accepted).Result;
            ViewBag.NumberOfServices = services;

            //Serviços Cancelados
            int[] cancelled = new int[5];
            cancelled[0] = NumberOfServicesCancelled(DateTime.Now.Add(new TimeSpan(-1, 0, 0, 0))).Result;
            cancelled[1] = NumberOfServicesCancelled(DateTime.Now.Add(new TimeSpan(-3, 0, 0, 0))).Result;
            cancelled[2] = NumberOfServicesCancelled(DateTime.Now.Add(new TimeSpan(-7, 0, 0, 0))).Result;
            cancelled[3] = NumberOfServicesCancelled(DateTime.Now.Add(new TimeSpan(-15, 0, 0, 0))).Result;
            cancelled[4] = NumberOfServicesCancelled(DateTime.Now.Add(new TimeSpan(-30, 0, 0, 0))).Result;
            ViewBag.NumberOfServicesCancelled = cancelled;

            ViewBag.TotalPointsUser = TotalPointsUser().Result;
            Dictionary<string, Dictionary<int, int>> positiveNegativeEvaluations = PositiveNegativeEvaluations().Result;
            ViewBag.PositiveNegativeEvaluations = positiveNegativeEvaluations;
            int negativeSum = 0;
            int positiveSum = 0;
            foreach (KeyValuePair<string, Dictionary<int, int>> pair in positiveNegativeEvaluations)
            {
                negativeSum += pair.Value.Keys.ElementAt(0);
                positiveSum += pair.Value.Values.ElementAt(0);
            }
            ViewBag.NegativeSum = negativeSum;
            ViewBag.PositiveSum = positiveSum;

            return View();
        }

        /// <summary>
        /// Gets average evaluation for each provider
        /// </summary>
        /// <param name="min">Minimum evaluation</param>
        /// <param name="max">Maximum evaluation</param>
        /// <returns>Dictionary<string, double></returns>
        public Dictionary<List<string>, List<double>> AverageEvaluationEachProvider(double min, double max)
        {
            Dictionary<List<string>, List<double>> info = new Dictionary<List<string>, List<double>>();
            Dictionary<string, double> avg = (from service in _context.Service
                    join evaluation in _context.CommentAndEvaluation
                    on service.ServiceID equals evaluation.ServiceID
                    group evaluation by service.UserID into grouped
                    select new
                    {
                        UserID = grouped.Key,
                        AverageEvaluation = Convert.ToDouble(Math.Round(grouped.Average(e => Convert.ToDecimal(e.Evaluation)), 2))
                    }).Where(q => q.AverageEvaluation >= min && q.AverageEvaluation <= max)
                    .OrderByDescending(q => q.AverageEvaluation)
                    .ToDictionary(q => q.UserID, q => q.AverageEvaluation);
            info.Add(avg.Keys.ToList(), avg.Values.ToList());
            return info;
        }

        /// <summary>
        /// Gets a list of providers with time
        /// </summary>
        /// <param name="min">Minimum date time</param>
        /// <returns>List<string></returns>
        public List<string> ListofProvidersWithTime(DateTime min)
        {
            List<User> users = (from user in _context.Users
                                join u_r in _context.UserRoles
                                on user.Id equals u_r.UserId
                                join role in _context.Roles on u_r.RoleId equals role.Id
                                where user.LastSeen >= min && role.Name == "Prestador"
                                select user).ToList();
            List<string> usersNames = new List<string>();
            users.ForEach(u => {
                usersNames.Add(u.FirstName + " " + u.LastName);
            });
            return usersNames;
        }

        /// <summary>
        /// Gets the number of clients with services with a status
        /// </summary>
        /// <param name="min">Minimum date time</param>
        /// <param name="status">Status of Service Requisition</param>
        /// <returns>int</returns>
        public int NumberOfClientsService(ServiceRequisitionStatus status, DateTime min)
        {
            int count = 0;
            count = (from serviceR in _context.ServiceRequisition
                     where serviceR.ServiceRequisitionStatus == status && serviceR.LastUpdatedTime >= min
                     select serviceR).Select(sr => sr.RequisitionerID).Distinct().Count();
            return count;
        }

        /// <summary>
        /// Number of users without activity
        /// </summary>
        /// <param name="date">Date time</param>
        /// <returns>int</returns>
        public int NumberOfUsersWithoutActivity(DateTime date)
        {
            int count = 0;
            count = (from user in _context.Users
                     where user.LastSeen < date
                     select user).Count();
            return count;
        }

        /// <summary>
        /// Total number of complaints of a certain type
        /// </summary>
        /// <param name="type">Type of Complaint</param>
        /// <returns>int</returns>
        public int NumberOfComplaints(ComplaintType type)
        {
            int count = 0;
            count = (from complaint in _context.Complaint
                     where complaint.ComplaintType == type
                     select complaint).Count();
            return count;
        }

        /// <summary>
        /// Total number of requisitions
        /// </summary>
        /// <param name="date">date time</param>
        /// <returns>int</returns>
        public int NumberOfRequisition(DateTime date)
        {
            int count = 0;
            count = (from r in _context.ServiceRequisition
                     where r.CreationDate >= date
                     select r).Count();
            return count;
        }

        /// <summary>
        /// Total number of requisitions completed
        /// </summary>
        /// <param name="date">date time</param>
        /// <returns>int</returns>
        public int NumberOfRequisitionCompleted(DateTime date)
        {
            int count = 0;
            count = (from r in _context.ServiceRequisition
                     where r.ServiceRequisitionStatus == ServiceRequisitionStatus.Concluded && r.LastUpdatedTime >= date
                     select r).Count();
            return count;
        }

        /// <summary>
        /// Average users age
        /// </summary>
        /// <returns>double</returns>
        public double AverageAge()
        {
            double avg = 0.00;
            avg = (from user in _context.Users
                    let years = DateTime.Now.Year - user.Birthdate.Year
                    let birthdayThisYear = user.Birthdate.AddYears(years)
                    select new
                    {
                        Age = birthdayThisYear > DateTime.Now ? years - 1 : years
                    }).Average(p => p.Age);
            return Convert.ToDouble(Math.Round(avg, 2));
        }

        /// <summary>
        /// Number of clients
        /// </summary>
        /// <param name="date">Date Time</param>
        /// <returns>int</returns>
        public async Task<int> NumberOfClients(DateTime date)
        {
            var user = await _userManager.GetUserAsync(User);
            int count = 0;
            count = (from r in _context.ServiceRequisition
                     join s in _context.Service
                     on r.ServiceID equals s.ServiceID
                     where s.UserID == user.Id && r.CreationDate >= date
                     select new {
                         RequisitionerID = r.RequisitionerID
                     }).Distinct().Count();
            return count;
        }

        /// <summary>
        /// Number of services
        /// </summary>
        /// <param name="status">Status of Service Requisition</param>
        /// <returns>int</returns>
        public async Task<int> NumberOfServices(ServiceRequisitionStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            int count = 0;
            count = (from r in _context.ServiceRequisition
                     join s in _context.Service
                     on r.ServiceID equals s.ServiceID
                     where s.UserID == user.Id && r.ServiceRequisitionStatus == status
                     select new
                     {
                         RequisitionerID = r.RequisitionerID
                     }).Count();
            return count;
        }

        /// <summary>
        /// Number os services cancelled
        /// </summary>
        /// <param name="date">Date Time</param>
        /// <returns>int</returns>
        public async Task<int> NumberOfServicesCancelled(DateTime date)
        {
            var user = await _userManager.GetUserAsync(User);
            int count = 0;
            count = (from service in _context.Service
                     join us in _context.Users on service.UserID equals us.Id
                     join s_r in _context.ServiceRequisition on service.ServiceID equals s_r.ServiceID
                     where s_r.LastUpdatedBy != us.UserName && s_r.ServiceRequisitionStatus == ServiceRequisitionStatus.Cancelled && service.UserID == user.Id && s_r.CreationDate >= date
                     select s_r).Count();
            return count;
        }

        /// <summary>
        /// Total points user
        /// </summary>
        /// <returns>int</returns>
        public async Task<int> TotalPointsUser()
        {
            var user = await _userManager.GetUserAsync(User);
            int count = 0;
            count = (from us in _context.Users
                     where us.Id == user.Id
                     select us.TotalPoints).FirstOrDefault();
            return count;
        }

        /// <summary>
        /// Number of positive and negative evaluations
        /// </summary>
        /// <returns>Task<Dictionary<string, Dictionary<int, int>>></returns>
        public async Task<Dictionary<string, Dictionary<int, int>>> PositiveNegativeEvaluations()
        {
            var user = await _userManager.GetUserAsync(User);
            Dictionary<int, int> positive = (from service in _context.Service
                                                join evaluation in _context.CommentAndEvaluation
                                                on service.ServiceID equals evaluation.ServiceID
                                                where evaluation.Evaluation >= 3 && service.UserID == user.Id
                                                group evaluation by service.ServiceID into grouped
                                                select new {
                                                    ServiceID = grouped.Key,
                                                    PositiveEvaluation = grouped.Count()
                                                }).ToDictionary(q => q.ServiceID, q => q.PositiveEvaluation);
            Dictionary<int, int> negative = (from service in _context.Service
                                             join evaluation in _context.CommentAndEvaluation
                                             on service.ServiceID equals evaluation.ServiceID
                                             where evaluation.Evaluation < 3 && service.UserID == user.Id
                                             group evaluation by service.ServiceID into grouped
                                             select new
                                             {
                                                 ServiceID = grouped.Key,
                                                 NegativeEvaluation = grouped.Count()
                                             }).ToDictionary(q => q.ServiceID, q => q.NegativeEvaluation);
            Dictionary<int, string> serviceName = (from service in _context.Service
                                                   where service.UserID == user.Id
                                                   select new { 
                                                       ServiceID = service.ServiceID,
                                                       ServiceName = service.ServiceName
                                                   }).ToDictionary(q => q.ServiceID, q => q.ServiceName);
            
            Dictionary<string, Dictionary<int, int>> info = new Dictionary<string, Dictionary<int, int>>();
            foreach (KeyValuePair<int, string> name in serviceName)
            {
                Dictionary<int, int> values = new Dictionary<int, int>();
                int neg = 0;
                try {
                    neg = negative[name.Key];
                } catch (KeyNotFoundException e) {
                    neg = 0;
                }
                int pos = 0;
                try
                {
                    pos = positive[name.Key];
                } catch (KeyNotFoundException e) {
                    pos = 0;
                }
                values.Add(neg, pos);
                info.Add(name.Value, values);
            }
            return info;
        }

        /// <summary>
        /// Number of requisitions by user
        /// </summary>
        /// <param name="userID">User idnetification</param>
        /// <returns>int</returns>
        public int Requisitions(string userID)
        {
            int count = 0;
            count = (from r in _context.ServiceRequisition
                     where r.RequisitionerID == userID && r.ServiceRequisitionStatus == ServiceRequisitionStatus.Concluded
                     select r).Count();
            return count;
        }

        /// <summary>
        /// This method returns the maximum number and subscriptions that exist.
        /// </summary>
        /// <returns>int</returns>
        public int MaxEnrolls()
        {
            int count = 0;
            count = (from formation in _context.Formation
                     select formation.NumberOfRegistrations).Max();
            return count;
        }

        /// <summary>
        /// This method returns the five most recent formations.
        /// </summary>
        /// <returns>Dictionary<string, int></returns>
        public Dictionary<List<string>, List<int>> NewestEnrolls()
        {
            Dictionary<List<string>, List<int>> dict = new Dictionary<List<string>, List<int>>();
            Dictionary<string, int> enrolls = (from formation in _context.Formation
                    orderby formation.Date descending
                    select new
                    {
                        Title = formation.Title,
                        Enrolls = formation.NumberOfRegistrations,
                    }).Take(5).OrderByDescending(q => q.Enrolls).ToDictionary(q => q.Title, q => q.Enrolls);
            if (enrolls.Count() < 5)
            {
                for (int i = enrolls.Count(); i < 5; i++)
                {
                    enrolls.Add("NotFound" + i.ToString(), 0);
                }
            }
            dict.Add(enrolls.Keys.ToList(), enrolls.Values.ToList());
            return dict;
        }

        /// <summary>
        /// Average number of enrollments in formations
        /// </summary>
        /// <returns>double</returns>
        public double AverageEnrolls()
        {
            double avg = 0.00;
            avg = (from formation in _context.Formation
                   select formation.NumberOfRegistrations).Average();
            return Convert.ToDouble(Math.Round(avg, 2));
        }

        /// <summary>
        /// This method returns the total average age of all users, the average age of users with the role client and the average age of users with role provider.
        /// </summary>
        /// <returns>double[]</returns>
        public double[] Ages()
        {
            double[] ages = new double[2];
            double avgCliente = 0.00;
            avgCliente = (from user in _context.Users
                   join r_u in _context.UserRoles on user.Id equals r_u.UserId
                   join role in _context.Roles on r_u.RoleId equals role.Id
                   where role.Name == "Cliente"
                   let years = DateTime.Now.Year - user.Birthdate.Year
                   let birthdayThisYear = user.Birthdate.AddYears(years)
                   select new
                   {
                       Age = birthdayThisYear > DateTime.Now ? years - 1 : years
                   }).Average(p => p.Age);
            ages[0] = Convert.ToDouble(Math.Round(avgCliente, 2));
            double avgPrestador = 0.00;
            avgPrestador = (from user in _context.Users
                          join r_u in _context.UserRoles on user.Id equals r_u.UserId
                          join role in _context.Roles on r_u.RoleId equals role.Id
                          where role.Name == "Prestador"
                          let years = DateTime.Now.Year - user.Birthdate.Year
                          let birthdayThisYear = user.Birthdate.AddYears(years)
                          select new
                          {
                              Age = birthdayThisYear > DateTime.Now ? years - 1 : years
                          }).Average(p => p.Age);
            ages[1] = Convert.ToDouble(Math.Round(avgPrestador, 2));
            return ages;
        }

        private int CountTheProviders()
        {
            return (from user in _context.Users
                    join r_u in _context.UserRoles on user.Id equals r_u.UserId
                    join role in _context.Roles on r_u.RoleId equals role.Id
                    where role.Name == "Prestador"
                    select user).Count();
        }
    }
}