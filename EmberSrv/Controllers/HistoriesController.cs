using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using TestApp.Models;

namespace TestApp.Controllers
{
    public class HistoriesController : ODataController
    {
        HistoriesContext db = new HistoriesContext();

        private bool HistoryExists(int key)
        {
            return db.Histories.Any(p => p.Id == key);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [EnableQuery]
        public IQueryable<History> Get()
        {
            return db.Histories;
        }

        [EnableQuery]
        public IQueryable<History> Get([FromODataUri] int key)
        {
            IQueryable<History> result = db.Histories.Where(p => p.BicId == key);
            return result;
        }

        public async Task<IHttpActionResult> Post(History history)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            String curDate = DateTime.Now.ToString();
            history.Start_date = curDate.Substring(0, curDate.Length -3);

            db.Histories.Add(history);
            await db.SaveChangesAsync();
            return Created(history);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<History> history)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = db.Histories.Where(k => k.BicId == key).Where(d => d.End_dep == 0).Single();
            if (entity == null)
            {
                return NotFound();
            }
            history.Patch(entity);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HistoryExists(key))
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
            var history = await db.Histories.FindAsync(key);
            if (history == null)
            {
                return NotFound();
            }
            db.Histories.Remove(history);
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}