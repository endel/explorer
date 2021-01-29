﻿using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

internal class ScenesViewController : IDisposable
{
    public event Action<Dictionary<string, SceneCardView>> OnDeployedScenesSet;
    public event Action<SceneCardView> OnDeployedSceneAdded;
    public event Action<SceneCardView> OnDeployedSceneRemoved;
    public event Action<Dictionary<string, SceneCardView>> OnProjectScenesSet;
    public event Action<SceneCardView> OnProjectSceneAdded;
    public event Action<SceneCardView> OnProjectSceneRemoved;

    private Dictionary<string, SceneCardView> deployedScenes = new Dictionary<string, SceneCardView>();
    private Dictionary<string, SceneCardView> projectScenes = new Dictionary<string, SceneCardView>();

    private readonly ScenesRefreshHelper scenesRefreshHelper = new ScenesRefreshHelper();
    private readonly SceneCardView sceneCardViewPrefab;

    public ScenesViewController(SceneCardView sceneCardViewPrefab)
    {
        this.sceneCardViewPrefab = sceneCardViewPrefab;
    }

    public void SetScenes(List<ISceneData> scenesData)
    {
        scenesRefreshHelper.Set(deployedScenes, projectScenes);

        deployedScenes = new Dictionary<string, SceneCardView>();
        projectScenes = new Dictionary<string, SceneCardView>();

        // update or create new scenes view
        for (int i = 0; i < scenesData.Count; i++)
        {
            SetScene(scenesData[i]);
        }

        // remove old deployed scenes
        if (scenesRefreshHelper.oldDeployedScenes != null)
        {
            using (var iterator = scenesRefreshHelper.oldDeployedScenes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    OnDeployedSceneRemoved?.Invoke(iterator.Current.Value);
                    DestroyCardView(iterator.Current.Value);
                }
            }
        }

        // remove old project scenes
        if (scenesRefreshHelper.oldProjectsScenes != null)
        {
            using (var iterator = scenesRefreshHelper.oldProjectsScenes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    OnProjectSceneRemoved?.Invoke(iterator.Current.Value);
                    DestroyCardView(iterator.Current.Value);
                }
            }
        }

        // notify scenes set if needed
        if (scenesRefreshHelper.isOldDeployedScenesEmpty && deployedScenes.Count > 0)
        {
            OnDeployedScenesSet?.Invoke(deployedScenes);
        }

        if (scenesRefreshHelper.isOldProjectScenesEmpty && projectScenes.Count > 0)
        {
            OnProjectScenesSet?.Invoke(projectScenes);
        }
    }

    public void Dispose()
    {
        using (var iterator = deployedScenes.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                Object.Destroy(iterator.Current.Value.gameObject);
            }
        }

        deployedScenes.Clear();

        using (var iterator = projectScenes.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                Object.Destroy(iterator.Current.Value.gameObject);
            }
        }

        projectScenes.Clear();
    }

    public void SetListener(IDeployedSceneListener listener)
    {
        listener.OnSetScenes(deployedScenes);
    }

    public void SetListener(IProjectSceneListener listener)
    {
        listener.OnSetScenes(deployedScenes);
    }

    private void SetScene(ISceneData sceneData)
    {
        bool shouldNotifyAdd = (sceneData.isDeployed && !scenesRefreshHelper.isOldDeployedScenesEmpty) ||
                               (!sceneData.isDeployed && !scenesRefreshHelper.isOldProjectScenesEmpty);

        if (scenesRefreshHelper.IsSceneDeployStatusChanged(sceneData))
        {
            ChangeSceneDeployStatus(sceneData, shouldNotifyAdd);
        }
        else if (scenesRefreshHelper.IsSceneUpdate(sceneData))
        {
            UpdateScene(sceneData);
        }
        else
        {
            CreateScene(sceneData, shouldNotifyAdd);
        }
    }

    private void ChangeSceneDeployStatus(ISceneData sceneData, bool shouldNotifyAdd)
    {
        var cardView = RemoveScene(sceneData, true);
        cardView.Setup(sceneData);
        AddScene(sceneData, cardView, shouldNotifyAdd);
    }

    private void UpdateScene(ISceneData sceneData)
    {
        var cardView = RemoveScene(sceneData, false);
        cardView.Setup(sceneData);
        AddScene(sceneData, cardView, false);
    }

    private void CreateScene(ISceneData sceneData, bool shouldNotifyAdd)
    {
        var cardView = CreateCardView();
        cardView.Setup(sceneData);
        AddScene(sceneData, cardView, shouldNotifyAdd);
    }

    private SceneCardView RemoveScene(ISceneData sceneData, bool notify)
    {
        bool wasDeployed = scenesRefreshHelper.WasDeployedScene(sceneData);
        var dictionary = wasDeployed ? scenesRefreshHelper.oldDeployedScenes : scenesRefreshHelper.oldProjectsScenes;

        if (dictionary.TryGetValue(sceneData.id, out SceneCardView sceneCardView))
        {
            dictionary.Remove(sceneData.id);

            if (notify)
            {
                if (wasDeployed)
                    OnDeployedSceneRemoved?.Invoke(sceneCardView);
                else
                    OnProjectSceneRemoved?.Invoke(sceneCardView);
            }

            return sceneCardView;
        }

        return null;
    }

    private void AddScene(ISceneData sceneData, SceneCardView sceneCardView, bool notify)
    {
        var dictionary = sceneData.isDeployed ? deployedScenes : projectScenes;
        dictionary.Add(sceneData.id, sceneCardView);

        if (notify)
        {
            if (sceneData.isDeployed)
                OnDeployedSceneAdded?.Invoke(sceneCardView);
            else
                OnProjectSceneAdded?.Invoke(sceneCardView);
        }
    }

    private SceneCardView CreateCardView()
    {
        SceneCardView sceneCardView = Object.Instantiate(sceneCardViewPrefab);
        sceneCardView.gameObject.SetActive(false);
        return sceneCardView;
    }

    private void DestroyCardView(SceneCardView sceneCardView)
    {
        Object.Destroy(sceneCardView.gameObject);
    }
}