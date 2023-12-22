using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public abstract class BaseBehaviour : MonoBehaviour
    {
        private IEventsService _eventsService;
        protected IEventsService EventsService =>
            _eventsService ??=
            Single<IEventsService>();

        protected T Single<T>(bool includeInactive = false) => 
            Find<T>(includeInactive).Single();

        protected T SingleOrDefault<T>(bool includeInactive = false) =>
            Find<T>(includeInactive).SingleOrDefault();

        protected static IEnumerable<T> Find<T>(bool includeInactive = false) =>
            SceneManager
                .GetActiveScene()
                .GetRootGameObjects()
                .SelectMany(r => r.GetComponentsInChildren<T>(includeInactive));
    }
}