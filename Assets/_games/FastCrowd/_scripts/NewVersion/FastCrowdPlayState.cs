﻿using System;
using System.Collections.Generic;

namespace EA4S.FastCrowd
{
    public class FastCrowdPlayState : IGameState
    {
        CountdownTimer gameTime;
        FastCrowdGame game;

        float anturaTimer;
        bool isAnturaRunning = false;

        public FastCrowdPlayState(FastCrowdGame game)
        {
            this.game = game;

            gameTime = new CountdownTimer(UnityEngine.Mathf.Lerp(90.0f, 60.0f, FastCrowdConfiguration.Instance.Difficulty));
            gameTime.onTimesUp += OnTimesUp;

            gameTime.Reset();
        }

        public void EnterState()
        {
            game.QuestionManager.OnCompleted += OnQuestionCompleted;
            game.QuestionManager.OnDropped += OnAnswerDropped;

            /*
            List<ILivingLetterData> wrongAnswers = new List<ILivingLetterData>();

            for (int i = 0; 
                i < FastCrowdConfiguration.Instance.MaxNumbOfWrongLettersNoise && 
                i < game.QuestionNumber &&
                i < game.NoiseData.Count; i++)
            {
                wrongAnswers.Add(game.NoiseData[i]);
            }
            */


            if (game.CurrentChallenge != null)
                game.QuestionManager.StartQuestion(game.CurrentChallenge, game.NoiseData, game.QuestionNumber);
            else
                game.QuestionManager.Clean();

            // Reset game timer
            gameTime.Start();

            game.Context.GetTimerWidget().Show();
            game.Context.GetTimerWidget().SetDuration(gameTime.Duration);
            game.Context.GetTimerWidget().SetTime(gameTime.Time);

            StopAntura();
        }

        public void ExitState()
        {
            StopAntura();

            game.Context.GetTimerWidget().Hide();
            gameTime.Stop();
            game.QuestionManager.OnCompleted -= OnQuestionCompleted;
            game.QuestionManager.OnDropped -= OnAnswerDropped;
            game.QuestionManager.Clean();
        }
        
        void OnQuestionCompleted()
        {
            if (FastCrowdConfiguration.Instance.Variation == FastCrowdVariation.Spelling ||
                  FastCrowdConfiguration.Instance.Variation == FastCrowdVariation.Letter)
            {
                // In spelling and letter, increment score only when the full question is completed
                game.IncrementScore();
            }

            game.SetCurrentState(game.ResultState);
        }

        void OnAnswerDropped(bool result)
        {
            game.Context.GetCheckmarkWidget().Show(result);

            if (result &&
                (FastCrowdConfiguration.Instance.Variation != FastCrowdVariation.Spelling &&
                FastCrowdConfiguration.Instance.Variation != FastCrowdVariation.Letter)
                )
            {
                // In spelling and letter, increment score only when the full question is completed
                game.IncrementScore();
            }

            game.Context.GetAudioManager().PlaySound(result ? Sfx.OK : Sfx.KO);
        }

        void StopAntura()
        {
            isAnturaRunning = false;
            game.antura.SetAnturaTime(false);
            // Schedule next exit
            anturaTimer = UnityEngine.Mathf.Lerp(20, 10, FastCrowdConfiguration.Instance.Difficulty);
            
            game.Context.GetAudioManager().PlayMusic(Music.Theme3);
        }

        void StartAntura()
        {
            isAnturaRunning = true;
            game.antura.SetAnturaTime(true);
            // Schedule next duration
            anturaTimer = UnityEngine.Mathf.Lerp(20, 10, FastCrowdConfiguration.Instance.Difficulty);
            
            game.Context.GetAudioManager().PlayMusic(Music.MainTheme);
        }

        public void Update(float delta)
        {
            anturaTimer -= delta;

            if (anturaTimer <= 0.0f)
            {
                if (isAnturaRunning)
                    StopAntura();
                else
                    StartAntura();
            }

            gameTime.Update(delta);
            game.Context.GetTimerWidget().SetTime(gameTime.Time);
        }

        public void UpdatePhysics(float delta)
        {
        }

        void OnTimesUp()
        {
            // Time's up!
            game.isTimesUp = true;
            game.SetCurrentState(game.EndState);
        }
    }
}