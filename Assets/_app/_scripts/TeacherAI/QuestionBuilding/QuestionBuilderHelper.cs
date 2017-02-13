﻿using System.Collections.Generic;
using EA4S.Database;

namespace EA4S.Teacher
{
    /// <summary>
    /// Defines rules on how question packs can be generated for a specific mini game.
    /// </summary>
    public static class QuestionBuilderHelper
    {

        public static void SortPacksByDifficulty(List<QuestionPackData> packs)
        {
            packs.Sort((x, y) => (int)(GetAverageIntrinsicDifficulty(x) - GetAverageIntrinsicDifficulty(y)));
        }

        private static float GetAverageIntrinsicDifficulty(QuestionPackData pack)
        {
            float qDiff = 0;
            float cDiff = 0;

            float qWeight = 0.5f;
            float cWeight = 0.5f;

            if (pack.questions.Count > 0)
            {
                foreach (var q in pack.questions) qDiff += ((IVocabularyData)q).GetIntrinsicDifficulty();
                qDiff /= pack.questions.Count;
            }
            else
            {
                qWeight = 0;
            }

            if (pack.correctAnswers.Count > 0)
            {
                foreach (var c in pack.correctAnswers) cDiff += ((IVocabularyData)c).GetIntrinsicDifficulty();
                cDiff /= pack.correctAnswers.Count;
            }
            else
            {
                cWeight = 0;
            }

            return (qWeight * qDiff + cWeight * cDiff) / (qWeight + cWeight);
        }

    }

}