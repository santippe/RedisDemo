using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisDemo;
using RedisWebApi.Models;

namespace RedisWebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    public class Redis : Controller
    {
        private readonly RedisService _service;
        //private readonly ILogger<Redis> _logger;

        public Redis(RedisService service)
        {
            _service = service;
        }

        // GET: Redis/Details/5
        [HttpGet]
        public ActionResult Details(string key)
        {
            return Ok(_service.GetDbEntities<DumbObject>(key));
        }

        [HttpPost("{objname}")]
        public ActionResult Create(DumbObject obj, [FromRoute] string objname)
        {
            try
            {
                obj.LstUpdt = DateTime.UtcNow;
                if (_service.InsertDbEntities(objname, obj))
                    return Ok();
                else
                    return StatusCode(500, "Cannot insert object");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{objname}")]
        public ActionResult Delete([FromRoute] string objname)
        {
            try
            {
                if (_service.DeleteDbEntities(objname))
                    return Ok();
                else
                    return StatusCode(500, "Cannot delete object");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public ActionResult GetAllKeys()
        {
            return Ok(_service.GetAllKeysAsync());
        }

        [HttpPost]
        public ActionResult DeleteAllKeys()
        {
            _service.DeleteAllKeys();
            return Ok();
        }
    }
}
