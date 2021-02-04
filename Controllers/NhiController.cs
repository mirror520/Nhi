using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Nhi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NhiController : ControllerBase
    {
        private readonly ILogger<NhiController> _logger;

        public NhiController(ILogger<NhiController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            var result = new Result<string>()
            {
                Status = "success",
                Info = new string[]{"連線成功"},
                Data = "PONG",
                Time = DateTime.Now
            };

            return Ok(result);
        }

        [HttpGet("User")]
        public IActionResult GetUser()
        {
            Result<User> result = null;

            var readerCtx = PCSC.ContextFactory.Instance.Establish(PCSC.SCardScope.User);
            var readerName = readerCtx.GetReaders().FirstOrDefault();
            if (string.IsNullOrEmpty(readerName))
            {
                result = new Result<User>()
                {
                    Status = "failure",
                    Info = new string[]{"找不到讀卡機"},
                    Time = DateTime.Now
                };

                return UnprocessableEntity(result);
            }

            try {
                var reader = new PCSC.Iso7816.IsoReader(
                    context: readerCtx, 
                    readerName: readerName, 
                    mode: PCSC.SCardShareMode.Shared,
                    protocol: PCSC.SCardProtocol.Any
                );
    
                var adpuInit = new PCSC.Iso7816.CommandApdu(PCSC.Iso7816.IsoCase.Case4Short, reader.ActiveProtocol)
                {
                    CLA = 0x00, 
                    INS = 0xA4, 
                    P1 = 0x04, 
                    P2 = 0x00, 
                    Data = new byte[] { 0xD1, 0x58, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11 }
                };
    
                var adpuInitResponse = reader.Transmit(adpuInit);
                if (!(adpuInitResponse.SW1.Equals(144) && adpuInitResponse.SW2.Equals(0)))
                {
                    result = new Result<User>()
                    {
                        Status = "failure",
                        Info = new string[]{"晶片卡非健保卡"},
                        Time = DateTime.Now
                    };
    
                    return UnprocessableEntity(result);
                }
    
                var adpuProfile = new PCSC.Iso7816.CommandApdu(PCSC.Iso7816.IsoCase.Case4Short, reader.ActiveProtocol)
                {
                    CLA = 0x00, 
                    INS = 0xCA, 
                    P1 = 0x11, 
                    P2 = 0x00, 
                    Data = new byte[] { 0x00, 0x00 }
                };
    
                var adpuProfileResponse = reader.Transmit(adpuProfile);
                if (adpuProfileResponse.HasData)
                {
                    var data = adpuProfileResponse.GetData();
                    var user = new User(data);
    
                    result = new Result<User>()
                    {
                        Status = "success",
                        Info = new string[]{"成功取得健保卡個人資料"},
                        Data = user,
                        Time = DateTime.Now
                    };
                }
    
                return Ok(result);
            } catch (PCSC.Exceptions.RemovedCardException) {
                result = new Result<User>()
                {
                    Status = "failure",
                    Info = new string[]{"未插入卡片"},
                    Time = DateTime.Now
                };

                return UnprocessableEntity(result);
            } catch (PCSC.Exceptions.UnresponsiveCardException) {
                result = new Result<User>()
                {
                    Status = "failure",
                    Info = new string[]{"無法預期錯誤，請拔除重試"},
                    Time = DateTime.Now
                };

                return UnprocessableEntity(result);
            }
        }
    }
}
