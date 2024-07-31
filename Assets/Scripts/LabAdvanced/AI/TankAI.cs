using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.Advanced
{
    public class TankAI : Agent
    {
        public float Rotate = 2f;
        public float MoveForce = 100f;
        public bool TrainingMode;

        private Rigidbody m_RBody;
        private bool m_Dead;

        public override void Initialize()
        {
            m_RBody = GetComponent<Rigidbody>();
        }

        public override void OnEpisodeBegin()
        {
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(this.transform.localPosition);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
        }
    }
}
