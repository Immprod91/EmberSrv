using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    public class BicyclesController : ODataController
    {
        BicyclesContext db = new BicyclesContext();

        private bool BicycleExists(int key)
        {
            return db.Bicycles.Any(p => p.Id == key);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [EnableQuery]
        public IQueryable<Bicycle> Get()
        {
            IQueryable<Bicycle> result = db.Bicycles;
            return result;
        }

        [EnableQuery]
        public SingleResult<Bicycle> Get([FromODataUri] int key)
        {
            IQueryable<Bicycle> result = db.Bicycles.Where(p => p.Id == key);
            HistoriesContext db_h = new HistoriesContext();
            double t;
            foreach (Bicycle b in result)
            {
                t = 0;
                foreach (History h in db_h.Histories.Where(h => h.BicId == b.Id))
                {
                    if ((h.End_date != null) && ((h.Start_date != null)))
                    {
                        int y = int.Parse(h.End_date.Split(' ')[0].Split('.')[2]) - int.Parse(h.Start_date.Split(' ')[0].Split('.')[2]);
                        int m = int.Parse(h.End_date.Split(' ')[0].Split('.')[1]) - int.Parse(h.Start_date.Split(' ')[0].Split('.')[1]);
                        int d = int.Parse(h.End_date.Split(' ')[0].Split('.')[0]) - int.Parse(h.Start_date.Split(' ')[0].Split('.')[0]);
                        int hour = int.Parse(h.End_date.Split(' ')[1].Split(':')[0]) - int.Parse(h.Start_date.Split(' ')[1].Split(':')[0]);
                        int min = int.Parse(h.End_date.Split(' ')[1].Split(':')[1]) - int.Parse(h.Start_date.Split(' ')[1].Split(':')[1]);
                        t += 24 * (y * 12 * 30 + m * 30 + d) + hour + min / 60.0;
                    }
                }
                if (t != 0) b.RentTime = t.ToString("#.##");
                else b.RentTime = "0";
            }
            return SingleResult.Create(result);
        }

        [ODataRoute("Bicycles/Default.GetByDep(id={id})")]
        [EnableQuery]
        public IQueryable<Bicycle> GetByDep(int id)
        {
            IQueryable<Bicycle> result = db.Bicycles.Where(p => p.DepId == id);
            return result;
        }

        public async Task<IHttpActionResult> Post(Bicycle bicycle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            db.Bicycles.Add(bicycle);
            await db.SaveChangesAsync();

            var path = System.Web.HttpContext.Current.Server.MapPath("~/Images/bic/");
            File.Copy(Path.Combine(path, "default.jpg"), Path.Combine(path, Created(bicycle).Entity.Id + ".jpg"), true);

            return Created(bicycle);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Bicycle> bicycle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Bicycles.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            bicycle.Patch(entity);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BicycleExists(key))
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

        public async Task<IHttpActionResult> Put([FromODataUri] int key, Bicycle update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != update.Id)
            {
                return BadRequest();
            }
            db.Entry(update).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BicycleExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(update);
        }

        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            var bicycle = await db.Bicycles.FindAsync(key);
            if (bicycle == null)
            {
                return NotFound();
            }

            var path = System.Web.HttpContext.Current.Server.MapPath("~/Images/bic/");
            File.Delete(Path.Combine(path, bicycle.Id + ".jpg"));

            db.Bicycles.Remove(bicycle);
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [ODataRoute("Bicycles/Default.GetLogo(id={id})")]
        [HttpGet]
        public HttpResponseMessage GetLogo(int id)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/Images/bic/" + id + ".jpg");

            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StreamContent(new FileStream(path, FileMode.Open));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }
    }
}