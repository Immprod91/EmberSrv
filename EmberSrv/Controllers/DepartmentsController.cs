using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TestApp.Models;

namespace TestApp.Controllers
{
    public class DepartmentsController: ODataController
    {
        DepartmentsContext db = new DepartmentsContext();

        private bool ProductExists(int key)
        {
            return db.Departments.Any(p => p.Id == key);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [EnableQuery]
        public IQueryable<Department> Get()
        {
            return db.Departments;
        }

        [EnableQuery]
        public SingleResult<Department> Get([FromODataUri] int key)
        {
            IQueryable<Department> result = db.Departments.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        public async Task<IHttpActionResult> Post(Department department)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            db.Departments.Add(department);
            await db.SaveChangesAsync();

            var path = System.Web.HttpContext.Current.Server.MapPath("~/Images/dep/");
            File.Copy(Path.Combine(path, "default.jpg"), Path.Combine(path, Created(department).Entity.Id + ".jpg"), true);

            return Created(department);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Department> department)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Departments.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            department.Patch(entity);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(entity);
        }

        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            var department = await db.Departments.FindAsync(key);
            if (department == null)
            {
                return NotFound();
            }

            var path = System.Web.HttpContext.Current.Server.MapPath("~/Images/dep/");
            File.Delete(Path.Combine(path, department.Id + ".jpg"));

            BicyclesContext db_b = new BicyclesContext();
            foreach (Bicycle b in db_b.Bicycles.Where(p => p.DepId == key))
            {
                b.DepId = null;
            }
            HistoriesContext db_h = new HistoriesContext();
            foreach (History h in db_h.Histories.Where(p => (p.End_dep == key || p.Start_dep == key)))
            {
                if (h.End_dep == key) h.End_dep = null;
                else h.Start_dep = null;
            }

            db.Departments.Remove(department);
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [ODataRoute("Departments/Default.GetLogo(id={id})")]
        [HttpGet]
        public HttpResponseMessage GetLogo(int id)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/Images/dep/" + id + ".jpg");

            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StreamContent(new FileStream(path, FileMode.Open));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }
    }
}