﻿using System;
using Firebase.Database;
using HashHunters.MinerMonitor.Common.Interfaces;
using System.Threading.Tasks;
using Firebase.Database.Query;
using HashHunters.MinerMonitor.Common.Extensions;

namespace HashHunters.MinerMonitor.Common
{
    public class FirebaseLogger : ILogger
    {
        private readonly TimeSpan WAIT_TIME = TimeSpan.FromSeconds(12);

        private readonly FirebaseClient FirebaseClient;
        private ChildQuery Root => FirebaseClient.Child("Rigs").Child(Environment.MachineName);

        public FirebaseLogger(IConfigProvider configProvider)
        {
            FirebaseClient = new FirebaseClient("https://rigcontrol-23592.firebaseio.com/",
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(configProvider.FirebaseKey) });
        }

        public void HealthCheck()
        {
            FirebasePut("HealthCheck", DateTime.Now.ToNice());
        }

        public void ServiceStart()
        {
            FirebasePut("LastStart", DateTime.Now.ToNice());
            FirebasePost("ServiceStarts", DateTime.Now.ToNice());
        }

        private void FirebasePut<T>(string path, T value)
        {
            try
            {
                Root.Child(path).PutAsync<T>(value).Wait(WAIT_TIME);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void FirebasePost<T>(string path, T value)
        {
            try
            {
                Root.Child(path).PostAsync<T>(value).Wait(WAIT_TIME);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
