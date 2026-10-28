using UnityEngine;

namespace Wildlands
{
    public sealed class Wildlife : MonoBehaviour
    {
        public HuntGame Game;
        public string Species;
        public bool Alive { get; private set; } = true;
        private Vector3 target;
        private float panicUntil;
        private float idleUntil;
        private float health;
        private Transform[] legs;
        public void Initialize(HuntGame game, string species, float hp, Transform[] limbs)
        {
            Game = game; Species = species; health = hp; legs = limbs;
            ChooseTarget();
        }
        private void ChooseTarget()
        {
            var direction = Random.insideUnitCircle * 18;
            target = new Vector3(Mathf.Clamp(transform.position.x + direction.x, -85, 85), 0,
                Mathf.Clamp(transform.position.z + direction.y, -85, 85));
        }
        public void HearShot(Vector3 position)
        {
            if (!Alive || Vector3.Distance(position, transform.position) > 95) return;
            Flee(position);
        }
        private void Flee(Vector3 from)
        {
            panicUntil = Time.time + 9;
            Vector3 away = transform.position - from;
            away.y = 0;
            if (away.sqrMagnitude < 0.01f) away = transform.forward;
            target = transform.position + away.normalized * 24;
            target.x = Mathf.Clamp(target.x, -90, 90);
            target.z = Mathf.Clamp(target.z, -90, 90);
        }
        public void Hit(float damage)
        {
            if (!Alive) return;
            health -= damage;
            if (health > 0) { Flee(Game.Player.position); return; }
            Alive = false;
            transform.rotation *= Quaternion.Euler(0, 0, 78);
            foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
            Game.RecordHarvest(Species);
            Destroy(gameObject, 25);
        }
        private void Update()
        {
            if (!Alive || !Game || !Game.Player) return;
            if (Vector3.Distance(transform.position, Game.Player.position) < 15 && Time.time > panicUntil)
                Flee(Game.Player.position);
            bool fleeing = Time.time < panicUntil;
            if (!fleeing && Time.time < idleUntil) return;
            Vector3 delta = target - transform.position;
            delta.y = 0;
            if (delta.magnitude < 1)
            {
                ChooseTarget();
                idleUntil = fleeing ? 0 : Time.time + Random.Range(2, 5);
                return;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), Time.deltaTime * 3);
            transform.position += transform.forward * ((fleeing ? 6 : 1.3f) * Time.deltaTime);
            if (legs != null)
                for (int i = 0; i < legs.Length; i++)
                    legs[i].localRotation = Quaternion.Euler(Mathf.Sin(Time.time * (fleeing ? 14 : 4) + i % 2 * Mathf.PI) * 23, 0, 0);
        }
    }
}
