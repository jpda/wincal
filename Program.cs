using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Azure.Storage.Blobs;

namespace wincal
{
    public static class Program
    {
        public static async Task Main()
        {
            await new Stuff().DoStuff();
            Console.WriteLine("fin");
            Console.ReadLine();
        }
    }

    public class Stuff
    {
        private IList<Appointment> _appointments = new List<Appointment>();
        public Stuff()
        {

        }

        public async Task DoStuff()
        {
            var users = await Windows.System.User
                .FindAllAsync(Windows.System.UserType.LocalUser,
                 Windows.System.UserAuthenticationStatus.LocallyAuthenticated
                );
            var store = await AppointmentManager.GetForUser(users[0])
                .RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            var cal = await store.GetAppointmentCalendarAsync("b,b,8d");
            Console.WriteLine($"opening calendar {cal.DisplayName}...");
            var appts = await cal.FindAppointmentsAsync(DateTimeOffset.Now, TimeSpan.FromDays(30));

            foreach (var a in appts)
            {
                _appointments.Add(a);
                Console.WriteLine($"{a.StartTime} {a.Subject} {a.Details} {a.Duration}");
            }

            var appointmentsToStore = appts.Select(x => new
            {
                x.StartTime,
                EndTime = x.StartTime.Add(x.Duration),
                x.Subject,
                x.Location
            });

            var serializedData = System.Text.Json.JsonSerializer.Serialize(appointmentsToStore);
            //System.IO.File.WriteAllText(@"d:\code\git\wincal\data.json", serializedData, System.Text.Encoding.UTF8);
            //Console.WriteLine(file.Path);

            var data = new System.BinaryData(System.Text.Encoding.UTF8.GetBytes(serializedData));

            var bStore = new BlobContainerClient("UseDevelopmentStorage=true", "eventdata");
            bStore.CreateIfNotExists();
            await bStore.UploadBlobAsync("data.json", data);
        }
    }
}