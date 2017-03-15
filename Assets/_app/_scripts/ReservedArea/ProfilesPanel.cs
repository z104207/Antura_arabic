﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EA4S.Core;
using EA4S.UI;
using EA4S.Profile;
using EA4S.Teacher;

namespace EA4S.ReservedArea
{
    public class ProfilesPanel : MonoBehaviour
    {
        [Header("References")]
        public TextRender PlayerInfoText;
        public GameObject PlayerIconContainer;
        public GameObject PlayerIconPrefab;
        public GameObject ProfileCommandsContainer;
        public GameObject pleaseWaitPanel;

        string SelectedPlayerId;

        void Start()
        {
            ResetAll();
        }

        void ResetAll()
        {
            SelectedPlayerId = "";
            RefreshPlayerIcons();
            RefreshUI();
        }

        void RefreshPlayerIcons()
        {
            GameObject newIcon;

            foreach (Transform t in PlayerIconContainer.transform) {
                Destroy(t.gameObject);
            }

            List<PlayerIconData> players = AppManager.I.PlayerProfileManager.GetSavedPlayers();

            // reverse the list for RIGHT 2 LEFT layout
            players.Reverse();
            foreach (var player in players) {
                newIcon = Instantiate(PlayerIconPrefab);
                newIcon.transform.SetParent(PlayerIconContainer.transform, false);
                newIcon.GetComponent<PlayerIcon>().Init(player);
                newIcon.GetComponent<UIButton>().Bt.onClick.AddListener(() => OnSelectPlayerProfile(player.Uuid));
            }
        }

        void RefreshUI()
        {
            // highlight selected profile
            ProfileCommandsContainer.SetActive(SelectedPlayerId != "");
            foreach (Transform t in PlayerIconContainer.transform) {
                t.GetComponent<PlayerIcon>().Select(SelectedPlayerId);
            }
        }

        public void OnSelectPlayerProfile(string uuid)
        {
            //Debug.Log("OnSelectPlayerProfile " + uuid);
            if (SelectedPlayerId != uuid) {
                SelectedPlayerId = uuid;
                PlayerInfoText.text = "player id: " + SelectedPlayerId;
            } else {
                SelectedPlayerId = "";
                PlayerInfoText.text = "";
            }
            RefreshUI();
        }

        public void OnOpenSelectedPlayerProfile()
        {
            //Debug.Log("OPEN " + SelectedPlayerId);
            AppManager.I.PlayerProfileManager.SetPlayerAsCurrentByUUID(SelectedPlayerId);
            AppManager.I.NavigationManager.GoToPlayerBook();
        }

        public void OnDeleteSelectPlayerProfile()
        {
            GlobalUI.ShowPrompt(Database.LocalizationDataId.UI_AreYouSure, DoDeleteSelectPlayerProfile, DoNothing);
        }

        void DoNothing()
        {

        }

        void DoDeleteSelectPlayerProfile()
        {
            //Debug.Log("DELETE " + SelectedPlayerId);
            AppManager.I.PlayerProfileManager.DeletePlayerProfile(SelectedPlayerId);
            ResetAll();
        }

        public void OnExportSelectPlayerProfile()
        {
            Debug.Log("EXPORT " + SelectedPlayerId);
        }

        public void OnCreateDemoPlayer()
        {
            if (AppManager.I.PlayerProfileManager.ExistsDemoUser()) {
                GlobalUI.ShowPrompt(Database.LocalizationDataId.ReservedArea_DemoUserAlreadyExists);
            } else {
                GlobalUI.ShowPrompt(Database.LocalizationDataId.UI_AreYouSure, DoCreateDemoPlayer, DoNothing);
            }
        }

        void DoCreateDemoPlayer()
        {
            StartCoroutine(CreateDemoPlayer());
        }

        public void OnImportProfile()
        {
            Debug.Log("IMPORT");
        }

        #region Demo User Helpers

        IEnumerator CreateDemoPlayer()
        {
            //Debug.Log("creating DEMO USER ");
            yield return null;
            activateWaitingScreen(true);
            yield return null;
            var demoUserUiid = AppManager.I.PlayerProfileManager.CreatePlayerProfile(10, PlayerGender.F, 1, PlayerTint.Red, true);
            SelectedPlayerId = demoUserUiid;

            // populate with fake data
            var maxJourneyPos = AppManager.I.JourneyHelper.GetFinalJourneyPosition();
            yield return StartCoroutine(PopulateDatabaseWithUsefulDataCO(maxJourneyPos));
            AppManager.I.Player.SetMaxJourneyPosition(maxJourneyPos, true);
            AppManager.I.Player.CheckGameFinished();                // force check
            AppManager.I.Player.CheckGameFinishedWithAllStars();    // force check
            Rewards.RewardSystemManager.UnlockAllRewards();

            ResetAll();
            activateWaitingScreen(false);
        }

        void activateWaitingScreen(bool status)
        {
            pleaseWaitPanel.gameObject.SetActive(status);
            GlobalUI.I.BackButton.gameObject.SetActive(!status);
        }

        IEnumerator PopulateDatabaseWithUsefulDataCO(JourneyPosition targetPosition)
        {
            bool useBestScores = true;

            var logAi = AppManager.I.Teacher.logAI;
            var fakeAppSession = LogManager.I.AppSession;

            // Add some mood data
            int nMoodData = 15;
            for (int i = 0; i < nMoodData; i++) {
                logAi.LogMood(0, Random.Range(AppConstants.minimumMoodValue, AppConstants.maximumMoodValue + 1));
                Debug.Log("Add mood " + i);
                yield return null;
            }

            // Add scores for all play sessions
            Debug.Log("Start adding PS scores");
            List<LogPlaySessionScoreParams> logPlaySessionScoreParamsList = new List<LogPlaySessionScoreParams>();
            var allPlaySessionInfos = AppManager.I.ScoreHelper.GetAllPlaySessionInfo();
            for (int i = 0; i < allPlaySessionInfos.Count; i++) {
                if (allPlaySessionInfos[i].data.Stage <= targetPosition.Stage) {
                    int score = useBestScores ? AppConstants.maximumMinigameScore : Random.Range(AppConstants.minimumMinigameScore, AppConstants.maximumMinigameScore);
                    logPlaySessionScoreParamsList.Add(new LogPlaySessionScoreParams(allPlaySessionInfos[i].data.GetJourneyPosition(), score, 12f));
                    //Debug.Log("Add play session score for " + allPlaySessionInfos[i].data.Id);
                }
            }
            logAi.LogPlaySessionScores(0, logPlaySessionScoreParamsList);
            Debug.Log("Finish adding PS scores");
            yield return null;

            // Add scores for all minigames
            Debug.Log("Start adding MiniGame scores");
            List<LogMiniGameScoreParams> logMiniGameScoreParamses = new List<LogMiniGameScoreParams>();
            var allMiniGameInfo = AppManager.I.ScoreHelper.GetAllMiniGameInfo();
            for (int i = 0; i < allMiniGameInfo.Count; i++) {
                int score = useBestScores ? AppConstants.maximumMinigameScore : Random.Range(AppConstants.minimumMinigameScore, AppConstants.maximumMinigameScore);
                logMiniGameScoreParamses.Add(new LogMiniGameScoreParams(JourneyPosition.InitialJourneyPosition, allMiniGameInfo[i].data.Code, score, 12f));
                //Debug.Log("Add minigame score " + i);
            }
            logAi.LogMiniGameScores(0, logMiniGameScoreParamses);
            Debug.Log("Finish adding MiniGame scores");
            yield return null;

            // Add scores for some learning data (words/letters/phrases)
            /*var maxPlaySession = AppManager.I.Player.MaxJourneyPosition.ToString();
            var allWordInfo = AppManager.I.Teacher.ScoreHelper.GetAllWordInfo();
            for (int i = 0; i < allWordInfo.Count; i++)
            {
                if (Random.value < 0.3f)
                {
                    var resultsList = new List<Teacher.LogAI.LearnResultParameters>();
                    var newResult = new Teacher.LogAI.LearnResultParameters();
                    newResult.elementId = allWordInfo[i].data.Id;
                    newResult.table = DbTables.Words;
                    newResult.nCorrect = Random.Range(1,5);
                    newResult.nWrong = Random.Range(1, 5);
                    resultsList.Add(newResult);
                    logAi.LogLearn(fakeAppSession, maxPlaySession, MiniGameCode.Assessment_LetterForm, resultsList);
                }
            }
            var allLetterInfo = AppManager.I.Teacher.ScoreHelper.GetAllLetterInfo();
            for (int i = 0; i < allLetterInfo.Count; i++)
            {
                if (Random.value < 0.3f)
                {
                    var resultsList = new List<Teacher.LogAI.LearnResultParameters>();
                    var newResult = new Teacher.LogAI.LearnResultParameters();
                    newResult.elementId = allLetterInfo[i].data.Id;
                    newResult.table = DbTables.Letters;
                    newResult.nCorrect = Random.Range(1, 5);
                    newResult.nWrong = Random.Range(1, 5);
                    resultsList.Add(newResult);
                    logAi.LogLearn(fakeAppSession, maxPlaySession, MiniGameCode.Assessment_LetterForm, resultsList);
                }
            }*/

        }

        #endregion
    }
}