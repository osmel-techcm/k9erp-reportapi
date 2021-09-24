using reportCore.DTOs;
using reportCore.Entities;
using reportCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WG3000_COMM.Core;

namespace reportShared.Services
{
    public class TerminalControllerService : ITerminalControllerService
    {
        private readonly ITerminalRepo _terminalRepo;
        private readonly ICardFobService _cardFobService;
        private readonly IDoorTerminalRepo _doorTerminalRepo;
        private readonly IDoorTerminalReaderRepo _doorTerminalReaderRepo;
        private readonly ILogCardFobService _logCardFobService;

        public TerminalControllerService(ITerminalRepo terminalRepo, ICardFobService cardFobService, IDoorTerminalRepo doorTerminalRepo,
            IDoorTerminalReaderRepo doorTerminalReaderRepo, ILogCardFobService logCardFobService)
        {
            _terminalRepo = terminalRepo;
            _cardFobService = cardFobService;
            _doorTerminalRepo = doorTerminalRepo;
            _doorTerminalReaderRepo = doorTerminalReaderRepo;
            _logCardFobService = logCardFobService;
        }

        public async Task<responseData> CanConnect(Terminal terminal)
        {
            var responseData = new responseData();
            int itemp;

            try
            {
                var wgMjController = new wgMjController
                {
                    ControllerSN = Convert.ToInt32(terminal.SerialNumber),
                    IP = terminal.Ip,
                    PORT = terminal.Port
                };


                if (!int.TryParse(terminal.SerialNumber, out itemp))
                {
                    responseData.error = true;
                    responseData.errorValue = 2;
                    responseData.description = "The serial number is not valid";
                    return responseData;
                }
                else
                {
                    if (wgMjController.GetMjControllerRunInformationIP() < 0)
                    {
                        responseData.error = true;
                        responseData.errorValue = 2;
                        responseData.description = "Controller not found";
                        return responseData;
                    }
                }
            }
            catch (Exception e)
            {
                responseData.error = true;
                responseData.errorValue = 2;
                responseData.description = e.Message;
            }



            return responseData;
        }

        public async Task<responseData> OpenDoorRemote(DoorTerminal doorTerminal)
        {
            var responseData = new responseData
            {
                description = "Open!"
            };

            var terminalResponse = await _terminalRepo.GetTerminal(doorTerminal.IdController);
            if (terminalResponse.error)
            {
                return terminalResponse;
            }

            var terminal = (Terminal)terminalResponse.data;

            var terminalController = new wgMjController
            {
                ControllerSN = Convert.ToInt32(terminal.SerialNumber),
                IP = terminal.Ip,
                PORT = Convert.ToInt32(terminal.Port)
            };

            if (terminalController.RemoteOpenDoorIP(doorTerminal.IdDoor) > 0)
            {
                responseData.description = "Door Opened";
            }
            else
            {
                responseData.error = true;
                responseData.errorValue = 2;
                responseData.description = "Door not found!";
            }

            return responseData;
        }

        public async Task<responseData> UpdateDoorConfig(DoorTerminal doorTerminal)
        {
            var responseData = new responseData();

            var terminalResponse = await _terminalRepo.GetTerminal(doorTerminal.IdController);
            if (terminalResponse.error)
            {
                return terminalResponse;
            }

            var terminal = (Terminal)terminalResponse.data;

            var terminalController = new wgMjController
            {
                ControllerSN = Convert.ToInt32(terminal.SerialNumber),
                IP = terminal.Ip,
                PORT = Convert.ToInt32(terminal.Port)
            };

            var terminalConfigure = new wgMjControllerConfigure();
            terminalConfigure.DoorControlSet(doorTerminal.IdDoor, doorTerminal.DoorState);
            terminalConfigure.DoorDelaySet(doorTerminal.IdDoor, doorTerminal.DoorDelay);

            if (terminalController.UpdateConfigureIP(terminalConfigure) < 0)
            {
                responseData.error = true;
                responseData.errorValue = 2;
                responseData.description = "Door not found!";
                return responseData;
            }

            return responseData;
        }

        public async Task<responseData> UpdateControlTimeSegList(Terminal terminal, List<TimeProfile> timeProfiles)
        {
            var responseData = new responseData();

            var controlTimeSegList = new wgMjControllerTimeSegList();

            foreach (var tp in timeProfiles)
            {
                var mjCI = new MjControlTimeSeg
                {
                    ymdStart = tp.StartDate,
                    ymdEnd = tp.EndDate,
                    SegIndex = byte.Parse(tp.Id.ToString()),
                    TotalLimittedAccess = (byte)(0),
                    LimittedMode = 0,
                    nextSeg = byte.Parse("0"),
                    weekdayControl = 0
                };
                mjCI.weekdayControl += (byte)(tp.DMonday ? (1 << 0) : 0);
                mjCI.weekdayControl += (byte)(tp.DTuesday ? (1 << 1) : 0);
                mjCI.weekdayControl += (byte)(tp.DWednesday ? (1 << 2) : 0);
                mjCI.weekdayControl += (byte)(tp.DThursday ? (1 << 3) : 0);
                mjCI.weekdayControl += (byte)(tp.DFriday ? (1 << 4) : 0);
                mjCI.weekdayControl += (byte)(tp.DSaturday ? (1 << 5) : 0);
                mjCI.weekdayControl += (byte)(tp.DSunday ? (1 << 6) : 0);
                mjCI.hmsStart1 = new DateTime(tp.StartDate.Year, tp.StartDate.Month, tp.StartDate.Day, tp.TimeIntervalStart_00.Hours, tp.TimeIntervalStart_00.Minutes, tp.TimeIntervalStart_00.Seconds);
                mjCI.hmsEnd1 = new DateTime(tp.StartDate.Year, tp.StartDate.Month, tp.StartDate.Day, tp.TimeIntervalEnd_00.Hours, tp.TimeIntervalEnd_00.Minutes, tp.TimeIntervalEnd_00.Seconds);
                mjCI.hmsStart2 = new DateTime(tp.StartDate.Year, tp.StartDate.Month, tp.StartDate.Day, tp.TimeIntervalStart_01.Hours, tp.TimeIntervalStart_01.Minutes, tp.TimeIntervalStart_01.Seconds);
                mjCI.hmsEnd2 = new DateTime(tp.StartDate.Year, tp.StartDate.Month, tp.StartDate.Day, tp.TimeIntervalEnd_01.Hours, tp.TimeIntervalEnd_01.Minutes, tp.TimeIntervalEnd_01.Seconds);
                mjCI.hmsStart3 = new DateTime(tp.StartDate.Year, tp.StartDate.Month, tp.StartDate.Day, tp.TimeIntervalStart_02.Hours, tp.TimeIntervalStart_02.Minutes, tp.TimeIntervalStart_02.Seconds);
                mjCI.hmsEnd3 = new DateTime(tp.StartDate.Year, tp.StartDate.Month, tp.StartDate.Day, tp.TimeIntervalEnd_02.Hours, tp.TimeIntervalEnd_02.Minutes, tp.TimeIntervalEnd_02.Seconds);
                mjCI.LimittedAccess1 = 0;
                mjCI.LimittedAccess2 = 0;
                mjCI.LimittedAccess3 = 0;
                controlTimeSegList.AddItem(mjCI);
            }

            var updController = new wgMjController
            {
                ControllerSN = Convert.ToInt32(terminal.SerialNumber),
                IP = terminal.Ip.ToString(),
                PORT = terminal.Port
            };

            if (updController.UpdateControlTimeSegListIP(controlTimeSegList.ToByte()) <= 0)
            {
                responseData.error = true;
                responseData.errorValue = 2;
                responseData.description = "Failed to update the time list!";
                return responseData;
            }

            return responseData;
        }

        public async Task<responseData> UpdateConfigureIP(Terminal terminal)
        {
            var responseData = new responseData();

            var controlConfigure = new wgMjControllerConfigure
            {
                controlTaskList_enabled = 1
            };

            var control = new wgMjController
            {
                ControllerSN = Convert.ToInt32(terminal.SerialNumber),
                IP = terminal.Ip,
                PORT = terminal.Port
            };

            control.UpdateConfigureIP(controlConfigure);
            control.AdjustTimeIP(DateTime.Now);

            var permissionResponse = await _terminalRepo.GetPermissionByTerminal(terminal.Id);
            if (permissionResponse.error)
            {
                return permissionResponse;
            }

            var permission = (List<TerminalPermissionDTO>)permissionResponse.data;

            var pri = new wgMjControllerPrivilege();

            using (var dtPrivilege = new DataTable("Privilege"))
            {
                dtPrivilege.Columns.Add("f_CardNO", Type.GetType("System.UInt32"));
                dtPrivilege.Columns.Add("f_BeginYMD", Type.GetType("System.DateTime"));
                dtPrivilege.Columns.Add("f_EndYMD", Type.GetType("System.DateTime"));
                dtPrivilege.Columns.Add("f_PIN", Type.GetType("System.String"));
                dtPrivilege.Columns.Add("f_ControlSegID1", Type.GetType("System.Byte"));
                dtPrivilege.Columns["f_ControlSegID1"].DefaultValue = 0;
                dtPrivilege.Columns.Add("f_ControlSegID2", Type.GetType("System.Byte"));
                dtPrivilege.Columns["f_ControlSegID2"].DefaultValue = 0;
                dtPrivilege.Columns.Add("f_ControlSegID3", Type.GetType("System.Byte"));
                dtPrivilege.Columns["f_ControlSegID3"].DefaultValue = 0;
                dtPrivilege.Columns.Add("f_ControlSegID4", Type.GetType("System.Byte"));
                dtPrivilege.Columns["f_ControlSegID4"].DefaultValue = 0;

                DataRow dr;

                var permmissionGrouped = permission.GroupBy(x => x.CardFobNumber);

                foreach (var items in permmissionGrouped)
                {
                    dr = dtPrivilege.NewRow();
                    dr["f_CardNO"] = uint.Parse(items.Key.ToString());
                    dr["f_BeginYMD"] = Convert.ToDateTime(items.FirstOrDefault().StarDate.ToString());
                    dr["f_EndYMD"] = Convert.ToDateTime(items.FirstOrDefault().EnDate.ToString());
                    dr["f_PIN"] = 345678;

                    var segId = 4;
                    foreach (var itemData in items)
                    {
                        var timeSegment = string.Empty;
                        if (string.IsNullOrEmpty(itemData.TimeProfile.ToString()))
                        {
                            timeSegment = "1";
                        }
                        else
                        {
                            timeSegment = itemData.TimeProfile.ToString();
                        }
                        dr[segId] = byte.Parse(timeSegment);
                        segId++;
                    }
                    dtPrivilege.Rows.Add(dr);
                }

                dtPrivilege.AcceptChanges();

                pri.AllowUpload();

                var resultUpload = pri.UploadIP(Convert.ToInt32(terminal.SerialNumber), terminal.Ip, terminal.Port, "DOOR NAME", dtPrivilege);

            }

            return responseData;
        }

        public async Task<responseData> downloadDataTerminals()
        {
            var responseData = new responseData
            {
                description = "Done!"
            };

            var listDataTables = new List<DataTable>();

            var terminals = await _terminalRepo.GetAllTerminals();

            try
            {
                foreach (var term in terminals)
                {
                    DataTable dtSwipeRecord;
                    dtSwipeRecord = new DataTable("SwipeRecord");
                    dtSwipeRecord.Columns.Add("f_Index", Type.GetType("System.UInt32"));
                    dtSwipeRecord.Columns.Add("f_ReadDate", Type.GetType("System.DateTime"));
                    dtSwipeRecord.Columns.Add("f_CardNO", Type.GetType("System.UInt32"));
                    dtSwipeRecord.Columns.Add("f_DoorNO", Type.GetType("System.UInt32"));
                    dtSwipeRecord.Columns.Add("f_InOut", Type.GetType("System.UInt32"));
                    dtSwipeRecord.Columns.Add("f_ReaderNO", Type.GetType("System.UInt32"));
                    dtSwipeRecord.Columns.Add("f_EventCategory", Type.GetType("System.UInt32"));
                    dtSwipeRecord.Columns.Add("f_ReasonNo", Type.GetType("System.UInt32"));
                    dtSwipeRecord.Columns.Add("f_ControllerSN", Type.GetType("System.UInt32"));
                    dtSwipeRecord.Columns.Add("f_RecordAll", Type.GetType("System.String"));

                    using (wgMjControllerSwipeOperate swipe4GetRecords = new wgMjControllerSwipeOperate())
                    {
                        swipe4GetRecords.Clear();
                        var num = swipe4GetRecords.GetSwipeRecords(Convert.ToInt32(term.SerialNumber), term.Ip, Convert.ToInt32(term.Port), ref dtSwipeRecord);
                        if (num > 0)
                        {
                            listDataTables.Add(dtSwipeRecord);
                            using (var _wgMjController = new wgMjController())
                            {
                                _wgMjController.ControllerSN = Convert.ToInt32(term.SerialNumber);
                                _wgMjController.IP = term.Ip;
                                _wgMjController.PORT = Convert.ToInt32(term.Port);
                                _wgMjController.RestoreAllSwipeInTheControllersIP();
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                //This is a temporary solution because the dll is a netframework project needs to be recompiled
                if (e.Message == "Thread abort is not supported on this platform.")
                {
                    wgMjControllerSwipeRecord mjrec = new wgMjControllerSwipeRecord();

                    foreach (DataTable tables in listDataTables)
                    {
                        var terminalSn = tables.Rows[0]["f_ControllerSN"].ToString();
                        var dbterminal = await _terminalRepo.GetTerminalBySerialNumber(terminalSn);
                        if (dbterminal == null)
                        {
                            continue;
                        }

                        foreach (DataRow row in tables.Rows)
                        {
                            mjrec.Update(row["f_RecordAll"].ToString());

                            var cardDb = await _cardFobService.GetCardFobByNumber(mjrec.CardID.ToString());
                            if (cardDb == null)
                            {
                                continue;
                            }

                            var doorTerminal = await _doorTerminalRepo.GetDoorTerminalByTerminalAndDoor(dbterminal.Id, mjrec.DoorNo);
                            if (doorTerminal == null)
                            {
                                continue;
                            }

                            var doorReadersResponse = await _doorTerminalReaderRepo.GetDoorTerminalReadersByDoor(doorTerminal.Id);
                            if (doorReadersResponse.error)
                            {
                                continue;
                            }

                            var doorReaders = (List<DoorTerminalReader>)doorReadersResponse.data;
                            var doorReader = doorReaders.FirstOrDefault(x => x.IdDoor == mjrec.DoorNo);

                            var logCardFob = new LogCardFob
                            {
                                IdCardFob = cardDb.Id,
                                IdDoor = doorTerminal.Id,
                                TimeLog = mjrec.ReadDate,
                                SwipeStatus = mjrec.ToDisplaySimpleInfo(false).Split(':')[4].ToString(),
                                IdReader = doorReader.Id
                            };

                            await _logCardFobService.PostLogCardFob(logCardFob);
                        }
                    }
                }
            }

            return responseData;
        }

        public async Task<dynamic> GetDoorStatus(dynamic dataReport)
        {
            foreach (var row in dataReport)
            {
                try
                {
                    using (var wgMjController = new wgMjController())
                    {
                        wgMjControllerConfigure conf = new wgMjControllerConfigure();
                        wgMjController.ControllerSN = Convert.ToInt32(row["SerialNumber"].ToString());
                        wgMjController.IP = row["Ip"].ToString();
                        wgMjController.PORT = Convert.ToInt32(row["Port"].ToString());

                        var response = (wgMjController.GetConfigureIP(ref conf) > 0);
                        switch (conf.DoorControlGet(Convert.ToInt32(row["IdDoor"].ToString())))
                        {
                            case 0:
                                row.Add("doorStatus", "Not controlled");
                                break;
                            case 1:
                                row.Add("doorStatus", "Open");
                                break;
                            case 2:
                                row.Add("doorStatus", "Closed");
                                break;
                            case 3:
                                row.Add("doorStatus", "Online");
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    //This is a temporary solution because the dll is a netframework project needs to be recompiled
                    var aa = e;
                }
            }

            return dataReport;
        }
    }
}
