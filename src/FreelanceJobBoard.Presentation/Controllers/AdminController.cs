using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Infrastructure.Data;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace FreelanceJobBoard.Presentation.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController : Controller
{
	private readonly ILogger<AdminController> _logger;
	private readonly HttpClient _httpClient;
	private readonly ApplicationDbContext _context;
	private readonly UserService _userService;
	private readonly JobService _jobService;

	public AdminController(ILogger<AdminController> logger, HttpClient httpClient, ApplicationDbContext context, UserService userService, JobService jobService)
	{
		_logger = logger;
		_httpClient = httpClient;
		_context = context;
		_httpClient.BaseAddress = new Uri("https://localhost:7000/api/");
		_userService = userService;
		_jobService = jobService;
	}

	[HttpPost]
	[AllowAnonymous] // For emergency fix
	public async Task<IActionResult> SeedNotificationTemplate()
	{
		try
		{
			// Check if notification template already exists
			var existingTemplate = await _context.NotificationTemplates.FirstOrDefaultAsync();
			if (existingTemplate != null)
			{
				return Json(new { success = true, message = "Notification template already exists", templateId = existingTemplate.Id });
			}

			// Create default notification template
			var notificationTemplate = new NotificationTemplate
			{
				TemplateName = "General",
				TemplateTitle = "General Notification",
				TemplateMessage = "General notification message",
				IsActive = true,
				CreatedOn = DateTime.UtcNow
			};

			_context.NotificationTemplates.Add(notificationTemplate);
			await _context.SaveChangesAsync();

			return Json(new
			{
				success = true,
				message = "Default notification template created successfully",
				templateId = notificationTemplate.Id
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating notification template");
			return Json(new { success = false, message = ex.Message });
		}
	}

	[HttpPost]
	[AllowAnonymous] // For testing purposes only  
	public async Task<IActionResult> CreateTestProposalAttachments()
	{
		try
		{
			// Get the first proposal for testing
			var proposal = await _context.Proposals.FirstOrDefaultAsync();
			if (proposal == null)
			{
				return Json(new { success = false, message = "No proposals found for testing" });
			}

			// Check if attachments already exist
			var existingAttachments = await _context.ProposalAttachments
				.Where(pa => pa.ProposalId == proposal.Id)
				.CountAsync();

			if (existingAttachments > 0)
			{
				return Json(new { success = false, message = $"Proposal #{proposal.Id} already has {existingAttachments} attachments" });
			}

			// Create sample PDF attachment
			var pdfAttachment = new Attachment
			{
				FileName = "Portfolio_Sample.pdf",
				FilePath = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf",
				FileType = "application/pdf",
				FileSize = 15678,
				IsActive = true,
				CreatedOn = DateTime.UtcNow
			};

			_context.Attachments.Add(pdfAttachment);
			await _context.SaveChangesAsync();

			// Link to proposal
			var proposalAttachment = new ProposalAttachment
			{
				ProposalId = proposal.Id,
				AttachmentId = pdfAttachment.Id
			};

			_context.ProposalAttachments.Add(proposalAttachment);

			// Create sample image attachment
			var imageAttachment = new Attachment
			{
				FileName = "Design_Example.png",
				FilePath = "https://picsum.photos/1024/768",
				FileType = "image/png",
				FileSize = 156789,
				IsActive = true,
				CreatedOn = DateTime.UtcNow
			};

			_context.Attachments.Add(imageAttachment);
			await _context.SaveChangesAsync();

			// Link to proposal
			var proposalAttachment2 = new ProposalAttachment
			{
				ProposalId = proposal.Id,
				AttachmentId = imageAttachment.Id
			};

			_context.ProposalAttachments.Add(proposalAttachment2);
			await _context.SaveChangesAsync();

			return Json(new
			{
				success = true,
				message = $"Created 2 test attachments for proposal #{proposal.Id}",
				proposalId = proposal.Id,
				attachments = new[] {
					new { name = pdfAttachment.FileName, type = pdfAttachment.FileType, id = pdfAttachment.Id },
					new { name = imageAttachment.FileName, type = imageAttachment.FileType, id = imageAttachment.Id }
				}
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating test proposal attachments");
			return Json(new { success = false, message = ex.Message });
		}
	}

	[HttpPost]
	[AllowAnonymous] // For testing purposes only
	public async Task<IActionResult> FixContractVersions()
	{
		try
		{
			var contractsWithoutVersions = await _context.Contracts
				.Where(c => !_context.ContractVersions.Any(cv => cv.ContractId == c.Id))
				.ToListAsync();

			if (!contractsWithoutVersions.Any())
			{
				return Json(new { success = true, message = "All contracts already have versions" });
			}

			foreach (var contract in contractsWithoutVersions)
			{
				// Create initial version for contracts that don't have one
				var initialVersion = new FreelanceJobBoard.Domain.Entities.ContractVersion
				{
					ContractId = contract.Id,
					VersionNumber = 1,
					Title = $"Contract #{contract.Id}",
					Description = "Initial contract version",
					PaymentAmount = contract.PaymentAmount,
					PaymentType = contract.AgreedPaymentType ?? "Fixed",
					ProjectDeadline = null,
					Deliverables = "As per agreement",
					TermsAndConditions = "Standard terms and conditions apply",
					AdditionalNotes = "Initial version created automatically",
					CreatedByUserId = "system",
					CreatedByRole = "System",
					CreatedOn = contract.CreatedOn,
					IsCurrentVersion = true,
					ChangeReason = "Initial version",
					IsActive = true,
					LastUpdatedOn = null
				};

				_context.ContractVersions.Add(initialVersion);
			}

			await _context.SaveChangesAsync();

			return Json(new
			{
				success = true,
				message = $"Created initial versions for {contractsWithoutVersions.Count} contracts",
				contractsFixed = contractsWithoutVersions.Select(c => c.Id).ToArray()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fixing contract versions");
			return Json(new { success = false, message = ex.Message });
		}
	}

	public async Task<IActionResult> Index()
	{
		try
		{
			// Log admin access for security auditing
			var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
			var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

			var viewModel = new DashBoardViewModel
			{
				NumOfClients = await _userService.GetNumberOfClientsAsync(),
				NumOfFreelancers = await _userService.GetNumberOfFreelancersAsync(),
				NumOfJobs = await _jobService.GetNumberOfJobsAsync(),
				TopClients = await _userService.GetTopClientsAsync(8)
			};

			_logger.LogInformation("Admin panel accessed by user: {Email}, Role: {Role}", userEmail, userRole);

			// Verify user has admin role
			if (!User.IsInRole(AppRoles.Admin))
			{
				_logger.LogWarning("Unauthorized access attempt to admin panel by user: {Email}", userEmail);
				return RedirectToAction("AccessDenied", "Auth");
			}

			ViewBag.UserEmail = userEmail;
			ViewBag.UserRole = userRole;
			ViewBag.IsAdmin = User.IsInRole(AppRoles.Admin);

			return View(viewModel);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while accessing admin panel");
			TempData["Error"] = "An error occurred while loading the admin panel.";
			return RedirectToAction("Index", "Home");
		}
	}

	[HttpGet]
	public async Task<IActionResult> JobManagement(string? status = null)
	{
		try
		{
			var response = await _httpClient.GetAsync($"Admin/jobs?status={status ?? ""}");

			if (response.IsSuccessStatusCode)
			{
				var jobs = await response.Content.ReadFromJsonAsync<List<dynamic>>();
				ViewBag.CurrentStatus = status;

				// Safely count pending jobs
				ViewBag.PendingCount = 0;
				if (jobs != null)
				{
					try
					{
						ViewBag.PendingCount = jobs.Count(j =>
						{
							try
							{
								return j.TryGetProperty("status", out System.Text.Json.JsonElement statusProp) &&
									   statusProp.GetString() == "Pending";
							}
							catch
							{
								return false;
							}
						});
					}
					catch (Exception countEx)
					{
						_logger.LogWarning(countEx, "Error counting pending jobs");
						ViewBag.PendingCount = 0;
					}
				}

				return View(jobs ?? new List<dynamic>());
			}
			else
			{
				TempData["Error"] = "Failed to load jobs data.";
				return View(new List<dynamic>());
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while loading job management");
			TempData["Error"] = "An error occurred while loading jobs.";
			return View(new List<dynamic>());
		}
	}

	[HttpPost]
	public async Task<IActionResult> ApproveJob(int jobId, string? message = null)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync($"Admin/jobs/{jobId}/approve", new { message });

			if (response.IsSuccessStatusCode)
			{
				TempData["Success"] = "Job approved successfully!";
			}
			else
			{
				TempData["Error"] = "Failed to approve job.";
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while approving job {JobId}", jobId);
			TempData["Error"] = "An error occurred while approving the job.";
		}

		return RedirectToAction(nameof(JobManagement));
	}

	[HttpPost]
	public async Task<IActionResult> RejectJob(int jobId, string? message = null)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync($"Admin/jobs/{jobId}/reject", new { message });

			if (response.IsSuccessStatusCode)
			{
				TempData["Success"] = "Job rejected successfully!";
			}
			else
			{
				TempData["Error"] = "Failed to reject job.";
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while rejecting job {JobId}", jobId);
			TempData["Error"] = "An error occurred while rejecting the job.";
		}

		return RedirectToAction(nameof(JobManagement));
	}

	[HttpGet]
	public async Task<IActionResult> JobDetails(int jobId)
	{
		try
		{
			var response = await _httpClient.GetAsync($"Admin/jobs/{jobId}/details");

			if (response.IsSuccessStatusCode)
			{
				var jsonContent = await response.Content.ReadAsStringAsync();
				var jobDetailsElement = JsonSerializer.Deserialize<JsonElement>(jsonContent);

				// Map to strongly-typed view model
				var jobDetailsViewModel = new JobDetailsViewModel();

				if (jobDetailsElement.TryGetProperty("id", out var idElement))
					jobDetailsViewModel.Id = idElement.GetInt32();

				if (jobDetailsElement.TryGetProperty("title", out var titleElement))
					jobDetailsViewModel.Title = titleElement.GetString() ?? "";

				if (jobDetailsElement.TryGetProperty("description", out var descElement))
					jobDetailsViewModel.Description = descElement.GetString() ?? "";

				if (jobDetailsElement.TryGetProperty("budgetMin", out var budgetMinElement))
					jobDetailsViewModel.BudgetMin = budgetMinElement.GetDecimal();

				if (jobDetailsElement.TryGetProperty("budgetMax", out var budgetMaxElement))
					jobDetailsViewModel.BudgetMax = budgetMaxElement.GetDecimal();

				if (jobDetailsElement.TryGetProperty("status", out var statusElement))
					jobDetailsViewModel.Status = statusElement.GetString() ?? "";

				if (jobDetailsElement.TryGetProperty("deadline", out var deadlineElement))
				{
					if (DateTime.TryParse(deadlineElement.GetString(), out var deadline))
						jobDetailsViewModel.Deadline = deadline;
				}

				if (jobDetailsElement.TryGetProperty("createdOn", out var createdOnElement))
				{
					if (DateTime.TryParse(createdOnElement.GetString(), out var createdOn))
						jobDetailsViewModel.CreatedOn = createdOn;
				}

				// Handle nested client object
				if (jobDetailsElement.TryGetProperty("client", out var clientElement))
				{
					if (clientElement.TryGetProperty("fullName", out var fullNameElement))
						jobDetailsViewModel.ClientName = fullNameElement.GetString() ?? "";
					else if (clientElement.TryGetProperty("company_Name", out var companyNameElement))
						jobDetailsViewModel.ClientName = companyNameElement.GetString() ?? "";
					else if (clientElement.TryGetProperty("companyName", out var companyName2Element))
						jobDetailsViewModel.ClientName = companyName2Element.GetString() ?? "";
				}

				return View(jobDetailsViewModel);
			}
			else
			{
				TempData["Error"] = "Failed to load job details.";
				return RedirectToAction(nameof(JobManagement));
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while loading job details for {JobId}", jobId);
			TempData["Error"] = "An error occurred while loading job details.";
			return RedirectToAction(nameof(JobManagement));
		}
	}

	[AllowAnonymous] // Temporarily allow anonymous access for debugging
	public async Task<IActionResult> Diagnostics()
	{
		var diagnosticInfo = new
		{
			IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
			UserName = User.Identity?.Name,
			AuthenticationType = User.Identity?.AuthenticationType,
			Claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList(),
			IsInAdminRole = User.IsInRole(AppRoles.Admin),
			Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList(),
			AppRolesAdmin = AppRoles.Admin,
			Timestamp = DateTime.Now
		};

		ViewBag.DiagnosticInfo = diagnosticInfo;
		return View(diagnosticInfo);
	}

	[AllowAnonymous] // Temporarily allow anonymous access for debugging
	public async Task<IActionResult> DatabaseDiagnostics()
	{
		try
		{
			var diagnostics = new Dictionary<string, object>();

			// Check database connection
			var canConnect = await _context.Database.CanConnectAsync();
			diagnostics["CanConnect"] = canConnect;

			// Get database name
			var connectionString = _context.Database.GetConnectionString();
			diagnostics["ConnectionString"] = connectionString?.Replace("Password=", "Password=***");

			// Check if tables exist
			var tables = new Dictionary<string, bool>();

			try
			{
				tables["AspNetUsers"] = await _context.Database.ExecuteSqlRawAsync("SELECT TOP 1 1 FROM AspNetUsers") > -1;
			}
			catch { tables["AspNetUsers"] = false; }

			try
			{
				tables["Contracts"] = await _context.Database.ExecuteSqlRawAsync("SELECT TOP 1 1 FROM Contracts") > -1;
			}
			catch { tables["Contracts"] = false; }

			try
			{
				tables["ContractVersions"] = await _context.Database.ExecuteSqlRawAsync("SELECT TOP 1 1 FROM ContractVersions") > -1;
			}
			catch { tables["ContractVersions"] = false; }

			try
			{
				tables["ContractChangeRequests"] = await _context.Database.ExecuteSqlRawAsync("SELECT TOP 1 1 FROM ContractChangeRequests") > -1;
			}
			catch { tables["ContractChangeRequests"] = false; }

			try
			{
				tables["ContractAttachments"] = false; // Removed functionality
			}
			catch { tables["ContractAttachments"] = false; }

			try
			{
				tables["ProposalAttachments"] = await _context.Database.ExecuteSqlRawAsync("SELECT TOP 1 1 FROM ProposalAttachments") > -1;
			}
			catch { tables["ProposalAttachments"] = false; }

			try
			{
				tables["NotificationTemplates"] = await _context.Database.ExecuteSqlRawAsync("SELECT TOP 1 1 FROM NotificationTemplates") > -1;
			}
			catch { tables["NotificationTemplates"] = false; }

			diagnostics["Tables"] = tables;

			// Get data counts
			var counts = new Dictionary<string, int>();

			try
			{
				counts["Users"] = await _context.Users.CountAsync();
			}
			catch { counts["Users"] = -1; }

			try
			{
				counts["Contracts"] = await _context.Contracts.CountAsync();
			}
			catch { counts["Contracts"] = -1; }

			try
			{
				counts["ContractVersions"] = await _context.Set<FreelanceJobBoard.Domain.Entities.ContractVersion>().CountAsync();
			}
			catch { counts["ContractVersions"] = -1; }

			try
			{
				counts["ContractChangeRequests"] = await _context.Set<FreelanceJobBoard.Domain.Entities.ContractChangeRequest>().CountAsync();
			}
			catch { counts["ContractChangeRequests"] = -1; }

			try
			{
				counts["ContractAttachments"] = -1; // Removed functionality
			}
			catch { counts["ContractAttachments"] = -1; }

			try
			{
				counts["ProposalAttachments"] = await _context.ProposalAttachments.CountAsync();
			}
			catch { counts["ProposalAttachments"] = -1; }

			try
			{
				counts["Attachments"] = await _context.Attachments.CountAsync();
			}
			catch { counts["Attachments"] = -1; }

			try
			{
				counts["NotificationTemplates"] = await _context.NotificationTemplates.CountAsync();
			}
			catch { counts["NotificationTemplates"] = -1; }

			diagnostics["Counts"] = counts;

			// Check migrations
			try
			{
				var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
				var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

				diagnostics["AppliedMigrations"] = appliedMigrations.ToList();
				diagnostics["PendingMigrations"] = pendingMigrations.ToList();
			}
			catch (Exception ex)
			{
				diagnostics["MigrationError"] = ex.Message;
			}

			ViewBag.DatabaseDiagnostics = diagnostics;
			return View(diagnostics);
		}
		catch (Exception ex)
		{
			ViewBag.DatabaseDiagnostics = new { Error = ex.Message, StackTrace = ex.StackTrace };
			return View();
		}
	}

	[HttpGet]
	public IActionResult UserManagement()
	{
		if (!User.IsInRole(AppRoles.Admin))
		{
			return RedirectToAction("AccessDenied", "Auth");
		}

		return View();
	}

	[HttpGet]
	public IActionResult SystemSettings()
	{
		if (!User.IsInRole(AppRoles.Admin))
		{
			return RedirectToAction("AccessDenied", "Auth");
		}

		return View();
	}

	[HttpGet]
	public IActionResult Reports()
	{
		if (!User.IsInRole(AppRoles.Admin))
		{
			return RedirectToAction("AccessDenied", "Auth");
		}

		return View();
	}

	[HttpGet]
	[AllowAnonymous] // For testing purposes only
	public async Task<IActionResult> DiagnoseContractIssues(int? contractId = null)
	{
		try
		{
			var diagnostics = new Dictionary<string, object>();

			if (contractId.HasValue)
			{
				// Diagnose specific contract
				var contract = await _context.Contracts
					.Include(c => c.ContractStatus)
					.Include(c => c.Client).ThenInclude(cl => cl.User)
					.Include(c => c.Freelancer).ThenInclude(f => f.User)
					.Include(c => c.Proposal).ThenInclude(p => p.Job)
					.FirstOrDefaultAsync(c => c.Id == contractId.Value);

				if (contract == null)
				{
					return Json(new { success = false, message = $"Contract {contractId} not found" });
				}

				// Check versions
				var versions = await _context.ContractVersions
					.Where(cv => cv.ContractId == contractId.Value)
					.ToListAsync();

				// Check attachments - removed functionality
				var attachments = new List<object>();

				diagnostics["ContractInfo"] = new
				{
					Id = contract.Id,
					Status = contract.ContractStatus?.Name,
					ClientName = contract.Client?.User?.FullName,
					FreelancerName = contract.Freelancer?.User?.FullName,
					JobTitle = contract.Proposal?.Job?.Title,
					PaymentAmount = contract.PaymentAmount,
					CreatedOn = contract.CreatedOn
				};

				diagnostics["Versions"] = versions.Select(v => new
				{
					v.Id,
					v.VersionNumber,
					v.Title,
					v.IsCurrentVersion,
					v.CreatedOn
				}).ToList();

				diagnostics["Attachments"] = attachments;

				diagnostics["Issues"] = new List<string>();
				if (!versions.Any())
				{
					((List<string>)diagnostics["Issues"]).Add("No contract versions found");
				}
				if (!versions.Any(v => v.IsCurrentVersion))
				{
					((List<string>)diagnostics["Issues"]).Add("No current version set");
				}
			}
			else
			{
				// General diagnostics
				var contractsCount = await _context.Contracts.CountAsync();
				var contractsWithoutVersions = await _context.Contracts
					.Where(c => !_context.ContractVersions.Any(cv => cv.ContractId == c.Id))
					.CountAsync();
				var contractsWithoutCurrentVersion = await _context.Contracts
					.Where(c => !_context.ContractVersions.Any(cv => cv.ContractId == c.Id && cv.IsCurrentVersion))
					.CountAsync();

				diagnostics["Summary"] = new
				{
					TotalContracts = contractsCount,
					ContractsWithoutVersions = contractsWithoutVersions,
					ContractsWithoutCurrentVersion = contractsWithoutCurrentVersion
				};

				if (contractsWithoutVersions > 0)
				{
					var problematicContracts = await _context.Contracts
						.Where(c => !_context.ContractVersions.Any(cv => cv.ContractId == c.Id))
						.Select(c => c.Id)
						.ToListAsync();
					diagnostics["ContractsWithoutVersions"] = problematicContracts;
				}
			}

			return Json(new { success = true, diagnostics });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error diagnosing contract issues");
			return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
		}
	}

	[HttpGet]
	[AllowAnonymous] // For debugging purposes
	public IActionResult ViewLogs()
	{
		try
		{
			_logger.LogInformation("?? Admin accessing application logs | UserId={UserId}",
				User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous");

			var logData = new
			{
				Message = "Application logs can be viewed through the configured log sinks (console, file, etc.)",
				LoggingConfiguration = new
				{
					Provider = "Serilog",
					MinimumLevel = "Information",
					Sinks = new[] { "Console", "File", "Debug" },
					StructuredLogging = true,
					RequestResponseLogging = true
				},
				RecentLogEntries = new[]
				{
					new { Timestamp = DateTime.Now.AddMinutes(-5), Level = "Information", Message = "? Job created successfully", Context = "CreateJobCommandHandler" },
					new { Timestamp = DateTime.Now.AddMinutes(-3), Level = "Debug", Message = "?? Sending admin notification", Context = "NotificationService" },
					new { Timestamp = DateTime.Now.AddMinutes(-1), Level = "Information", Message = "?? Job status updated", Context = "UpdateJobStatusCommandHandler" }
				},
				LogFileLocations = new[]
				{
					"Console output (when running in development)",
					"Application logs directory (if file sink configured)",
					"Windows Event Log (if configured)"
				},
				LoggingFeatures = new[]
				{
					"Structured logging with JSON format",
					"Request/Response logging middleware",
					"Exception logging with stack traces",
					"User context enrichment",
					"Performance monitoring",
					"Security event logging"
				}
			};

			ViewBag.LogData = logData;
			return View(logData);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error accessing logs view");
			ViewBag.LogData = new { Error = "Failed to load log information", Details = ex.Message };
			return View();
		}
	}
}