using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Moonpop.Tests
{
    public sealed class AssetTests
    {
        [Test] public void ProjectUsesTheRequestedEditorVersion()
        {
            Assert.That(Application.unityVersion,Is.EqualTo("6000.6.5f1"));
            Assert.That(File.ReadAllText("ProjectSettings/ProjectVersion.txt"),Does.Contain("m_EditorVersion: 6000.6.5f1"));
        }
        [Test] public void SceneAndAllRuntimeResourcesExist()
        {
            Assert.That(File.Exists("Assets/Moonpop/Scenes/Moonpop.unity"),Is.True);
            Assert.That(Resources.Load<GameSettings>("GameSettings"),Is.Not.Null);
            foreach(string name in new[]{"MidnightCircuit","Engine","Coin","Fuel","Crash","Lane","Click","Firework"})
                Assert.That(Resources.Load<AudioClip>("Audio/"+name),Is.Not.Null,name);
            foreach(string name in new[]{"NeonSurface","NeonParticle","NightSky"})
                Assert.That(Resources.Load<Shader>("Rendering/"+name),Is.Not.Null,name);
        }
        [Test] public void ControlsUseThreeActionMapsAndRequiredBindings()
        {
            var asset=Resources.Load<InputActionAsset>("Input/DrivingControls");
            Assert.That(asset,Is.Not.Null);Assert.That(asset.actionMaps.Count,Is.EqualTo(3));
            Assert.That(asset.FindAction("Gameplay/Left").bindings.Count,Is.GreaterThanOrEqualTo(2));
            Assert.That(asset.FindAction("Gameplay/Right").bindings.Count,Is.GreaterThanOrEqualTo(2));
            Assert.That(asset.FindAction("UI/Click"),Is.Not.Null);
            Assert.That(asset.FindAction("System/Pause"),Is.Not.Null);
        }
        [Test] public void EveryShaderCompiles()
        {
            foreach(string name in new[]{"NeonSurface","NeonParticle","NightSky"})
            {
                Shader shader=Resources.Load<Shader>("Rendering/"+name);
                Assert.That(shader,Is.Not.Null);Assert.That(ShaderUtil.ShaderHasError(shader),Is.False,name);
            }
        }
    }
}
