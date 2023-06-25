using SurveyForm.Data;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace SurveyForm.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly SurveyFormDbEntities _db = new SurveyFormDbEntities();

		#region Home

		public ActionResult Index()
		{
			return View();
		}

		[AllowAnonymous]
		public ActionResult Survey(string q, string msg = "")
		{
			ViewBag.Msg = msg;

			var formDetails = _db.Surveys
				.Include(a => a.Template)
				.Include(a => a.Template.TemplateQuestions)
				.Include(a => a.Template.TemplateQuestions.Select(b => b.Question))
				.FirstOrDefault(a => a.FormGuid == q);

			ViewBag.AlreadySubmitted = _db.SurveyResponses.Any(a => a.FromIp == Request.UserHostAddress && a.SurveyId == formDetails.SurveyId);

			return View(formDetails);
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult Survey(Survey survey)
		{
			bool status = false;
			try
			{
				// add survey response
				var dbResponse = _db.SurveyResponses.Add(new SurveyRespons
				{
					SurveyId = survey.SurveyId,
					DateSubmitted = DateTime.Now,
					FromIp = Request.UserHostAddress
				});
				_db.SaveChanges();

				// add response answers
				var responseAnswers = new List<ResponseAnswer>();
				var questionKeys = Request.Form.AllKeys.Where(a => a.Contains("Question_"));
				foreach (var key in questionKeys)
				{
					int.TryParse(Request.Form.Get(key), out var questionId);

					if (questionId <= 0 || responseAnswers.Exists(a => a.QuestionId == questionId)) continue;

					var answer = Request.Form.Get(key.Replace("Question_", "Answer_"));

					if (string.IsNullOrEmpty(answer)) continue;

					responseAnswers.Add(new ResponseAnswer
					{
						QuestionId = questionId,
						ResponseId = dbResponse.ResponseId,
						Answer = answer
					});
				}
				_db.ResponseAnswers.AddRange(responseAnswers);
				_db.SaveChanges();

				status = true;
			}
			catch { }

			return RedirectToAction("Survey", new { q = survey.FormGuid, msg = status ? "Survey submitted successfully." : "Failed to submit survey." });
		}

		#endregion

		#region Surveys

		public ActionResult Forms(string msg = "")
		{
			ViewBag.Msg = msg;

			var formsList = _db.Surveys.Include(a => a.Template).ToList();
			return View(formsList);
		}

		public ActionResult CreateForm(string msg = "")
		{
			ViewBag.Msg = msg;

			var templatesList = _db.Templates.ToList();
			ViewBag.TemplatesList = templatesList;
			return View();
		}
		[HttpPost]
		public ActionResult CreateForm(Survey survey)
		{
			try
			{
				if (ModelState.IsValid)
				{
					survey.FormGuid = Guid.NewGuid().ToString();
					_db.Surveys.Add(survey);
					_db.SaveChanges();

					return RedirectToAction("Forms", new { msg = "Survey Form added sucessfully." });
				}
			}
			catch { }

			return RedirectToAction("CreateForm", new { msg = "Failed to add Survey Form." });
		}

		public ActionResult DeleteForm(int surveyId)
		{
			try
			{
				var survey = _db.Surveys.Find(surveyId);
				if (survey != null)
				{
					_db.Surveys.Remove(survey);
					_db.SaveChanges();
					return RedirectToAction("Forms", new { msg = "Survey Form deleted sucessfully." });
				}
			}
			catch { }

			return RedirectToAction("Forms", new { msg = "Failed to delete Survey Form." });
		}

		#endregion

		#region Templates

		public ActionResult Templates(string msg = "")
		{
			ViewBag.Msg = msg;

			var templateList = _db.Templates.ToList();
			return View(templateList);
		}
		public ActionResult TemplateDetail(int templateId)
		{
			var templateDetails = _db.Templates.Find(templateId);
			ViewBag.TemplateQuestions = _db.TemplateQuestions.Include(a => a.Question).Where(a => a.TemplateId == templateId).ToList();
			return View(templateDetails);
		}

		public ActionResult CreateTemplate(bool returnToCreateForm = false, string msg = "")
		{
			ViewBag.Msg = msg;
			ViewBag.ReturnToCreateForm = returnToCreateForm;

			var questionsList = _db.Questions.ToList();
			ViewBag.QuestionsList = questionsList;
			return View();
		}
		[HttpPost]
		public ActionResult CreateTemplate(Template template)
		{
			try
			{
				if (ModelState.IsValid)
				{
					// add template
					var dbTemplate = _db.Templates.Add(template);
					_db.SaveChanges();

					// add template questions
					var templateQuestions = new List<TemplateQuestion>();
					var questionKeys = Request.Form.AllKeys.Where(a => a.Contains("Question_"));
					foreach (var key in questionKeys)
					{
						int.TryParse(Request.Form.Get(key), out var questionId);

						if (questionId <= 0 || templateQuestions.Exists(a => a.QuestionId == questionId)) continue;

						templateQuestions.Add(new TemplateQuestion
						{
							QuestionId = questionId,
							TemplateId = dbTemplate.TemplateId
						});
					}
					_db.TemplateQuestions.AddRange(templateQuestions);
					_db.SaveChanges();

					bool.TryParse(Request.Form.Get("ReturnToCreateForm"), out var returnToCreateForm);

					return RedirectToAction(returnToCreateForm ? "CreateForm" : "Templates", new { msg = "Template added sucessfully." });
				}
			}
			catch { }

			return RedirectToAction("CreateTemplate", new { msg = "Failed to add Template." });
		}

		public ActionResult DeleteTemplate(int templateId)
		{
			try
			{
				var template = _db.Templates.Find(templateId);
				if (template != null)
				{
					_db.Templates.Remove(template);
					_db.SaveChanges();
					return RedirectToAction("Templates", new { msg = "Template deleted sucessfully." });
				}
			}
			catch { }

			return RedirectToAction("Templates", new { msg = "Failed to delete Template." });
		}

		#endregion

		#region Questions

		public ActionResult Questions(string msg = "")
		{
			ViewBag.Msg = msg;

			var questionsList = _db.Questions.ToList();
			return View(questionsList);
		}

		public ActionResult CreateQuestion(bool returnToCreateTemplate = false, string msg = "")
		{
			ViewBag.Msg = msg;
			ViewBag.ReturnToCreateTemplate = returnToCreateTemplate;

			return View();
		}
		[HttpPost]
		public ActionResult CreateQuestion(Question question)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_db.Questions.Add(question);
					_db.SaveChanges();

					bool.TryParse(Request.Form.Get("ReturnToCreateTemplate"), out var returnToCreateTemplate);

					return RedirectToAction(returnToCreateTemplate ? "CreateTemplate" : "Questions", new { msg = "Question added sucessfully." });
				}
			}
			catch { }

			return RedirectToAction("CreateQuestion", new { msg = "Failed to add Question." });
		}

		public ActionResult DeleteQuestion(int questionId)
		{
			try
			{
				var question = _db.Questions.Find(questionId);
				if (question != null)
				{
					_db.Questions.Remove(question);
					_db.SaveChanges();
					return RedirectToAction("Questions", new { msg = "Question deleted sucessfully." });
				}
			}
			catch { }

			return RedirectToAction("Questions", new { msg = "Failed to delete Question." });
		}

		#endregion
	}
}