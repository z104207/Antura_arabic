﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;
using EA4S.API;
using System.Collections.Generic;

namespace EA4S.Test
{

    public class MiniGameLauncher : MonoBehaviour
    {

        public MiniGamesDropDownList MiniGamesDropDownList;
        public Button LaunchButton;

        public void LaunchGame()
        {
            // Example minigame call
            MiniGameCode miniGameCodeSelected = (MiniGameCode)Enum.Parse(typeof(MiniGameCode), MiniGamesDropDownList.options[MiniGamesDropDownList.value].text);
            float difficulty = float.Parse(FindObjectsOfType<InputField>().First(n => n.name == "Difficulty").text);

            int packsCount = 0;

            switch (miniGameCodeSelected) {
                case MiniGameCode.Egg:
                    packsCount = 4;
                    break;
                case MiniGameCode.FastCrowd_alphabet:
                    packsCount = 1;
                    break;
                case MiniGameCode.DancingDots:
                case MiniGameCode.ThrowBalls_letters:
                case MiniGameCode.ThrowBalls_words:
                case MiniGameCode.Maze:
                case MiniGameCode.Balloons_counting:
                case MiniGameCode.Balloons_letter:
                case MiniGameCode.Balloons_spelling:
                case MiniGameCode.Balloons_words:
                case MiniGameCode.FastCrowd_letter:
                case MiniGameCode.FastCrowd_spelling:
                case MiniGameCode.FastCrowd_words:
                case MiniGameCode.FastCrowd_counting:
                case MiniGameCode.Tobogan_letters:
                case MiniGameCode.Tobogan_words:
                    packsCount = 10;
                    break;
                case MiniGameCode.Assessment_Letters:
                case MiniGameCode.Assessment_LettersMatchShape:
                case MiniGameCode.AlphabetSong:
                case MiniGameCode.ColorTickle:
                case MiniGameCode.DontWakeUp:
                case MiniGameCode.HiddenSource:
                case MiniGameCode.HideSeek:
                case MiniGameCode.MakeFriends:
                case MiniGameCode.MissingLetter:
                case MiniGameCode.MissingLetter_phrases:
                case MiniGameCode.MixedLetters_alphabet:
                case MiniGameCode.MixedLetters_spelling:
                case MiniGameCode.SickLetter:
                case MiniGameCode.ReadingGame:
                case MiniGameCode.Scanner:
                case MiniGameCode.Scanner_phrase:

                    break;

            }
            // Call start game with parameters
            MiniGameAPI.Instance.StartGame(
                miniGameCodeSelected,
                CreateQuestionPacksDummyAI(miniGameCodeSelected, packsCount),
                new GameConfiguration(difficulty)
            );

        }

        List<IQuestionPack> CreateQuestionPacksDummyAI(MiniGameCode _miniGameCode, int _questionNumber)
        {
            List<IQuestionPack> questionPackList = new List<IQuestionPack>();
            for (int i = 0; i < _questionNumber; i++) {
                questionPackList.Add(CreateQuestionPack(_miniGameCode));
            }
            return questionPackList;
        }

        IQuestionPack CreateQuestionPack(MiniGameCode _miniGameCode)
        {
            IQuestionPack questionPack = null;
            ILivingLetterData question;
            List<ILivingLetterData> correctAnswers = new List<ILivingLetterData>();
            List<ILivingLetterData> wrongAnswers = new List<ILivingLetterData>();
            List<LL_LetterData> letters = new List<LL_LetterData>();

            switch (_miniGameCode) {
                case MiniGameCode.Assessment_Letters:
                case MiniGameCode.Assessment_LettersMatchShape:
                    break;
                case MiniGameCode.AlphabetSong:
                    break;
                case MiniGameCode.Balloons_counting:
                    // Dummy logic for question creation
                    foreach (var w in AppManager.Instance.DB.GetAllWordData().Where(w => w.Category == Db.WordCategory.Number)) {
                        LL_WordData w_ll = new LL_WordData(w.Id, w);
                        correctAnswers.Add(w_ll);
                        if (correctAnswers.Count > 10)
                            break;
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(null, null, correctAnswers);
                    break;
                case MiniGameCode.Balloons_letter:
                    // Dummy logic for question creation
                    question = AppManager.Instance.Teacher.GimmeARandomLetter();

                    for (int i = 0; i < 1; i++) {
                        LL_WordData correctWord = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (CheckIfContains(GetLettersFromWord(correctWord), question))
                            correctAnswers.Add(correctWord);
                        else
                            i--;
                    }

                    for (int i = 0; i < 10; i++) {
                        LL_WordData wrongWord = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (!CheckIfContains(GetLettersFromWord(wrongWord), question))
                            wrongAnswers.Add(wrongWord);
                        else
                            i--;
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.Balloons_spelling:
                    // Dummy logic for question creation
                    letters = GetLettersFromWord(AppManager.Instance.Teacher.GimmeAGoodWordData());
                    foreach (var l in letters) {
                        correctAnswers.Add(l);
                    }
                    letters = GetLettersNotContained(letters, 8);
                    foreach (var l in letters) {
                        wrongAnswers.Add(l);
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(null, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.Balloons_words:
                    // Dummy logic for question creation
                    LL_WordData balloonWord = AppManager.Instance.Teacher.GimmeAGoodWordData();
                    correctAnswers.Add(balloonWord);
                    for (int i = 0; i < 10; i++) {
                        LL_WordData wrongWord = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (!correctAnswers.Contains(wrongWord))
                            wrongAnswers.Add(wrongWord);
                        else
                            i--;
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(balloonWord, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.ColorTickle:
                    break;
                case MiniGameCode.DancingDots:
                    // Dummy logic for question creation
                    question = AppManager.Instance.Teacher.GimmeARandomLetter();
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, null, null);
                    break;
                case MiniGameCode.DontWakeUp:
                    break;
                case MiniGameCode.Egg:
                    //
                    question = AppManager.Instance.Teacher.GimmeAGoodWordData();
                    letters = GetLettersFromWord(question as LL_WordData);
                    foreach (var l in letters) {
                        correctAnswers.Add(l);
                    }
                    // The AI in definitive version must check the difficulty threshold (0.5f in example) to determine gameplayType without passing wrongAnswers
                    if (float.Parse(FindObjectsOfType<InputField>().First(n => n.name == "Difficulty").text) < 0.5f) {
                        letters = GetLettersNotContained(letters, 7);
                        foreach (var l in letters) {
                            wrongAnswers.Add(l);
                        }
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.FastCrowd_alphabet:
                    // Dummy logic for get fake full ordered alphabet.
                    foreach (var letter in AppManager.Instance.DB.GetAllLetterData())
                        correctAnswers.Add(new LL_LetterData(letter.GetId()));

                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(null, null, correctAnswers);
                    break;
                case MiniGameCode.FastCrowd_counting:
                    // Dummy logic for question creation
                    LL_WordData word = AppManager.Instance.Teacher.GimmeAGoodWordData();
                    correctAnswers.Add(word);
                    for (int i = 0; i < 10; i++) {
                        LL_WordData wrongWord = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (!correctAnswers.Contains(wrongWord))
                            wrongAnswers.Add(wrongWord);
                        else
                            i--;
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(word, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.FastCrowd_letter:
                    // Dummy logic for question creation
                    question = AppManager.Instance.Teacher.GimmeARandomLetter();
                    letters = GetLettersNotContained(letters, 3); // TODO: auto generation in game
                    foreach (var l in letters) {
                        wrongAnswers.Add(l);
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.FastCrowd_spelling: // var 1
                    // Dummy logic for question creation
                    question = AppManager.Instance.Teacher.GimmeAGoodWordData();
                    letters = GetLettersFromWord(question as LL_WordData);
                    foreach (var l in letters) {
                        correctAnswers.Add(l);
                    }
                    letters = GetLettersNotContained(letters, 8);
                    foreach (var l in letters) {
                        wrongAnswers.Add(l);
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.FastCrowd_words:
                    // Dummy logic for question creation
                    for (int i = 0; i < 4; i++) {
                        LL_WordData correctWord = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (!CheckIfContains(correctAnswers, correctWord))
                            correctAnswers.Add(correctWord);
                        else
                            i--;
                    }
                    for (int i = 0; i < 2; i++) {
                        LL_WordData wrongWord = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (!CheckIfContains(correctAnswers, wrongWord))
                            wrongAnswers.Add(wrongWord);
                        else
                            i--;
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(null, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.HiddenSource:
                    break;
                case MiniGameCode.HideSeek:
                    break;
                case MiniGameCode.MakeFriends:
                    question = AppManager.Instance.Teacher.GimmeARandomLetter();
                    for (int i = 0; i < 2; i++) {
                        LL_WordData correctWordMakeFriends = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (!CheckIfContains(correctAnswers, correctWordMakeFriends) && CheckIfContains(GetLettersFromWord(correctWordMakeFriends), question))
                            // if not already in list and contain question letterData
                            correctAnswers.Add(correctWordMakeFriends);
                        else
                            i--;
                    }
                    for (int i = 0; i < 8; i++) {
                        LL_WordData wrongWordMakeFriends = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (!CheckIfContains(GetLettersFromWord(wrongWordMakeFriends), question))
                            // if not contain quest letter
                            wrongAnswers.Add(wrongWordMakeFriends);
                        else
                            i--;
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.Maze:
                    // Dummy logic for question creation
                    question = AppManager.Instance.Teacher.GimmeAGoodWordData();
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, null, null);
                    break;
                case MiniGameCode.MissingLetter:
                    break;
                case MiniGameCode.MissingLetter_phrases:
                    break;
                case MiniGameCode.MixedLetters_alphabet:
                    break;
                case MiniGameCode.MixedLetters_spelling:
                    break;
                case MiniGameCode.SickLetter:
                    break;
                case MiniGameCode.ReadingGame:
                    break;
                case MiniGameCode.Scanner:
                    // Dummy logic for question creation
                    question = AppManager.Instance.Teacher.GimmeAGoodWordData();

                    for (int i = 0; i < 8; i++) {
                        LL_WordData wrongWord = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (wrongWord != question)
                            wrongAnswers.Add(wrongWord);
                        else
                            i--;
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, wrongAnswers, null);
                    break;
                case MiniGameCode.Scanner_phrase:
                    // Wait Design info
                    break;
                case MiniGameCode.ThrowBalls_letters:
                    // Dummy logic for question creation
                    question = AppManager.Instance.Teacher.GimmeARandomLetter();
                    letters = GetLettersNotContained(letters, 3);
                    foreach (var l in letters) {
                        wrongAnswers.Add(l);
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, wrongAnswers, null);
                    break;
                case MiniGameCode.ThrowBalls_words:
                    // Dummy logic for question creation
                    question = AppManager.Instance.Teacher.GimmeAGoodWordData();
                    
                    for (int i = 0; i < 8; i++) {
                        LL_WordData wrongWord = AppManager.Instance.Teacher.GimmeAGoodWordData();
                        if (wrongWord != question)
                            wrongAnswers.Add(wrongWord);
                        else
                            i--;
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, wrongAnswers, null);
                    break;
                case MiniGameCode.Tobogan_letters:
                    // Dummy logic for question creation
                    question = AppManager.Instance.Teacher.GimmeAGoodWordData();
                    letters = GetLettersFromWord(question as LL_WordData);
                    foreach (var l in letters) {
                        correctAnswers.Add(l);
                        break; //get alway first (only for test)
                    }
                    letters = GetLettersNotContained(letters, 3);
                    foreach (var l in letters) {
                        wrongAnswers.Add(l);
                    }
                    // QuestionPack creation
                    questionPack = new FindRightDataQuestionPack(question, wrongAnswers, correctAnswers);
                    break;
                case MiniGameCode.Tobogan_words:
                    break;
                default:
                    break;
            }
            return questionPack;
        }

        #region Test Helpers

        LL_WordData GetWord()
        {
            return AppManager.Instance.Teacher.GimmeAGoodWordData();
        }

        List<LL_WordData> GetWordsNotContained(List<LL_WordData> _WordsToAvoid, int _count)
        {
            List<LL_WordData> wordListToReturn = new List<LL_WordData>();
            for (int i = 0; i < _count; i++) {
                var word = AppManager.Instance.Teacher.GimmeAGoodWordData();

                if (!CheckIfContains(_WordsToAvoid, word) && !CheckIfContains(wordListToReturn, word)) {
                    wordListToReturn.Add(word);
                }
            }
            return wordListToReturn;
        }

        List<LL_LetterData> GetLettersFromWord(LL_WordData _word)
        {
            List<LL_LetterData> letters = new List<LL_LetterData>();
            foreach (var letterData in ArabicAlphabetHelper.LetterDataListFromWord(_word.Data.Arabic, AppManager.Instance.Letters)) {
                letters.Add(letterData);
            }
            return letters;
        }

        List<LL_LetterData> GetLettersNotContained(List<LL_LetterData> _lettersToAvoid, int _count)
        {
            List<LL_LetterData> letterListToReturn = new List<LL_LetterData>();
            for (int i = 0; i < _count; i++) {
                var letter = AppManager.Instance.Teacher.GimmeARandomLetter();

                if (!CheckIfContains(_lettersToAvoid, letter) && !CheckIfContains(letterListToReturn, letter)) {
                    letterListToReturn.Add(letter);
                }
            }
            return letterListToReturn;
        }


        static bool CheckIfContains(List<ILivingLetterData> list, ILivingLetterData letter)
        {
            for (int i = 0, count = list.Count; i < count; ++i)
                if (list[i].Key == letter.Key)
                    return true;
            return false;
        }


        static bool CheckIfContains(List<LL_LetterData> list, ILivingLetterData letter)
        {
            for (int i = 0, count = list.Count; i < count; ++i)
                if (list[i].Key == letter.Key)
                    return true;
            return false;
        }

        static bool CheckIfContains(List<LL_WordData> list, ILivingLetterData letter)
        {
            for (int i = 0, count = list.Count; i < count; ++i)
                if (list[i].Key == letter.Key)
                    return true;
            return false;
        }

        #endregion
    }
}
