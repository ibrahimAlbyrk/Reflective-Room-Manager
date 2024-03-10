// Tests for [Server], [Client], [ServerCallback], [ClientCallback] attributes.
// Generated by AttributeTestGenerator.cs

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests.EditorBehaviours.Attributes
{
    public class AttributeTest_NetworkBehaviour
    {
        AttributeBehaviour_NetworkBehaviour behaviour;
        GameObject go;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject();
            go.AddComponent<NetworkIdentity>();
            behaviour = go.AddComponent<AttributeBehaviour_NetworkBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            NetworkClient.connectState = ConnectState.None;
            NetworkServer.active = false;
            UnityEngine.Object.DestroyImmediate(go);
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_float_returnsValue(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;
            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'System.Single Mirror.Tests.EditorBehaviours.Attributes.AttributeBehaviour_NetworkBehaviour::Client_float_Function()' called when client was not active");
            }
            float actual = behaviour.Client_float_Function();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Client_float_setsOutValue(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;
            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Client] function 'System.Void Mirror.Tests.EditorBehaviours.Attributes.AttributeBehaviour_NetworkBehaviour::Client_float_out_Function(System.Single&)' called when client was not active");
            }
            behaviour.Client_float_out_Function(out float actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_float_returnsValue(bool active)
        {
            NetworkServer.active = active;
            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'System.Single Mirror.Tests.EditorBehaviours.Attributes.AttributeBehaviour_NetworkBehaviour::Server_float_Function()' called when server was not active");
            }
            float actual = behaviour.Server_float_Function();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Server_float_setsOutValue(bool active)
        {
            NetworkServer.active = active;
            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;
            if (!active)
            {
                LogAssert.Expect(LogType.Warning, "[Server] function 'System.Void Mirror.Tests.EditorBehaviours.Attributes.AttributeBehaviour_NetworkBehaviour::Server_float_out_Function(System.Single&)' called when server was not active");
            }
            behaviour.Server_float_out_Function(out float actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_float_returnsValue(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;
            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;

            float actual = behaviour.ClientCallback_float_Function();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ClientCallback_float_setsOutValue(bool active)
        {
            NetworkClient.connectState = active ? ConnectState.Connected : ConnectState.None;
            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;

            behaviour.ClientCallback_float_out_Function(out float actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_float_returnsValue(bool active)
        {
            NetworkServer.active = active;
            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;

            float actual = behaviour.ServerCallback_float_Function();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ServerCallback_float_setsOutValue(bool active)
        {
            NetworkServer.active = active;
            float expected = active ? AttributeBehaviour_NetworkBehaviour.Expected_float : default;

            behaviour.ServerCallback_float_out_Function(out float actual);
            Assert.AreEqual(expected, actual);
        }
    }
}
