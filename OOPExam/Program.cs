using System;
using System.Collections.Generic;
using System.Linq;

namespace ExaminationSystem
{
    public enum QuestionType
    {
        TrueFalse = 1,
        MultipleChoice = 2
    }

    public enum ExamType
    {
        Final = 1,
        Practical = 2
    }

    public abstract class Question
    {
        public string Body { get; set; }
        public int Mark { get; set; }
        public List<Answer> Answers { get; set; }
        public int RightAnswer { get; set; }

        public Question()
        {
            Answers = new List<Answer>();
        }

        public abstract void Display();

        public virtual bool ValidateAnswer(int answer)
        {
            return answer >= 1 && answer <= Answers.Count;
        }
    }

    public class TFQuestion : Question
    {
        public TFQuestion()
        {
            Answers = new List<Answer>
            {
                new Answer(1, "True"),
                new Answer(2, "False")
            };
        }

        public override void Display()
        {
            Console.WriteLine($"\nQ: {Body} \n1. True / 2. False");
        }

        public override bool ValidateAnswer(int answer)
        {
            return answer == 1 || answer == 2;  // Only allow 1 (True) or 2 (False)
        }
    }

    public class MCQQuestion : Question
    {
        public override void Display()
        {
            Console.WriteLine($"\nQ: {Body}");
            foreach (var answer in Answers)
            {
                Console.WriteLine($"{answer.AnswerId}. {answer.AnswerText}");
            }
        }
    }

    public class Answer
    {
        public int AnswerId { get; }
        public string AnswerText { get; }

        public Answer(int id, string text)
        {
            AnswerId = id;
            AnswerText = text;
        }
    }

    public abstract class Exam
    {
        public TimeSpan Duration { get; set; }
        public int NumberOfQuestions { get; set; }
        public List<Question> Questions { get; } = new List<Question>();

        public abstract ExamResult ShowExam();

        protected DateTime StartTime { get; private set; }

        protected void StartExam()
        {
            StartTime = DateTime.Now;
            Console.WriteLine($"Exam started at: {StartTime:HH:mm:ss}");
            Console.WriteLine($"Duration: {Duration.TotalMinutes} minutes");
            Console.WriteLine($"End time: {StartTime + Duration:HH:mm:ss}\n");
        }

        protected bool IsTimeUp()
        {
            return DateTime.Now - StartTime > Duration;
        }
    }

    public class ExamResult
    {
        public int TotalScore { get; }
        public int MaxScore { get; }
        public TimeSpan TimeTaken { get; }

        public ExamResult(int totalScore, int maxScore, TimeSpan timeTaken)
        {
            TotalScore = totalScore;
            MaxScore = maxScore;
            TimeTaken = timeTaken;
        }

        public override string ToString()
        {
            return $"\nExam Results:" +
                   $"\nScore: {TotalScore}/{MaxScore} ({(double)TotalScore / MaxScore:P0})" +
                   $"\nTime taken: {TimeTaken.Minutes}m {TimeTaken.Seconds}s";
        }
    }

    public class FinalExam : Exam
    {
        public override ExamResult ShowExam()
        {
            StartExam();
            int totalScore = 0;
            int maxScore = 0;

            foreach (var question in Questions)
            {
                if (IsTimeUp())
                {
                    Console.WriteLine("\nTime's up! Exam terminated.");
                    break;
                }

                question.Display();
                var answer = ConsoleHelper.ReadInt("Your Answer: ", question.ValidateAnswer);
                maxScore += question.Mark;
                if (answer == question.RightAnswer)
                    totalScore += question.Mark;
            }

            var timeTaken = DateTime.Now - StartTime;
            return new ExamResult(totalScore, maxScore, timeTaken);
        }
    }

    public class PracticalExam : Exam
    {
        public override ExamResult ShowExam()
        {
            StartExam();
            int totalScore = 0;
            int maxScore = 0;

            foreach (var question in Questions)
            {
                question.Display();
                var answer = ConsoleHelper.ReadInt("Your Answer: ", question.ValidateAnswer);
                maxScore += question.Mark;
                if (answer == question.RightAnswer)
                {
                    totalScore += question.Mark;
                    Console.WriteLine("Correct!");
                }
                else
                {
                    Console.WriteLine($"Wrong. Correct answer: {question.RightAnswer}");
                }
            }

            var timeTaken = DateTime.Now - StartTime;
            return new ExamResult(totalScore, maxScore, timeTaken);
        }
    }

    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public Exam Exam { get; private set; }

        public void CreateExam()
        {
            var examType = ConsoleHelper.ReadEnum<ExamType>("Enter Exam Type (1- Final, 2- Practical): ");
            var timeInMinutes = ConsoleHelper.ReadInt("Enter Exam Time (minutes): ", x => x > 0);
            var numQuestions = ConsoleHelper.ReadInt("Enter Number of Questions: ", x => x > 0);

            Exam = examType == ExamType.Final ? new FinalExam() : new PracticalExam();
            Exam.Duration = TimeSpan.FromMinutes(timeInMinutes);
            Exam.NumberOfQuestions = numQuestions;

            for (int i = 0; i < numQuestions; i++)
            {
                Console.WriteLine($"\nQuestion {i + 1}:");
                CreateQuestion(Exam is PracticalExam);
            }
        }

        private void CreateQuestion(bool isPractical)
        {
            QuestionType qType;
            if (isPractical)
            {
                qType = QuestionType.MultipleChoice;
                Console.WriteLine("MCQ Questions: ");
            }
            else
            {
                qType = ConsoleHelper.ReadEnum<QuestionType>("Enter Question Type (1- TF, 2- MCQ): ");
            }

            var body = ConsoleHelper.ReadString("Enter Question Body: ");
            var mark = ConsoleHelper.ReadInt("Enter Question Mark: ", x => x > 0);

            Question question = qType == QuestionType.TrueFalse ? new TFQuestion() : new MCQQuestion();
            question.Body = body;
            question.Mark = mark;

            if (qType == QuestionType.MultipleChoice)
            {
                var numChoices = ConsoleHelper.ReadInt("Enter number of answer choices (2-4): ", x => x >= 2 && x <= 4);
                for (int j = 0; j < numChoices; j++)
                {
                    var ansText = ConsoleHelper.ReadString($"Enter Answer {j + 1}: ");
                    question.Answers.Add(new Answer(j + 1, ansText));
                }
            }

            if (qType == QuestionType.TrueFalse)
            {
                var correctAns = ConsoleHelper.ReadInt("Enter Correct Answer (1 for True, 2 for False): ",
                    x => x == 1 || x == 2);
                question.RightAnswer = correctAns;
            }
            else
            {
                var correctAns = ConsoleHelper.ReadInt("Enter Correct Answer Number: ",
                    x => x >= 1 && x <= question.Answers.Count);
                question.RightAnswer = correctAns;
            }

            Exam.Questions.Add(question);
        }
    }

    public static class ConsoleHelper
    {
        public static int ReadInt(string prompt, Func<int, bool> validator)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int value) && validator(value))
                    return value;
                Console.WriteLine("Invalid input. Please try again.");
            }
        }

        public static string ReadString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(input))
                    return input;
                Console.WriteLine("Input cannot be empty.");
            }
        }

        public static T ReadEnum<T>(string prompt) where T : struct
        {
            while (true)
            {
                Console.Write(prompt);
                if (Enum.TryParse<T>(Console.ReadLine(), out T result) &&
                    Enum.IsDefined(typeof(T), result))
                    return result;
                Console.WriteLine("Invalid input. Please try again.");
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Subject subject = new Subject();
            subject.CreateExam();
            Console.Clear();
            if (ConsoleHelper.ReadString("Start Exam? (y/n): ") == "y")
            {
                var result = subject.Exam.ShowExam();
                Console.WriteLine(result);
            }
        }
    }
}