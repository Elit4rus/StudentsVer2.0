using StudentsVer2._0.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StudentsVer2._0
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static StudentEntities context = new StudentEntities();
        public static class ImageHelper
        {
            // Загрузка изображений студента
            public static List<ImageDocument> GetStudentImages(int studentId)
            {
                return App.context.StudentImage
                    .Where(si => si.StudentID == studentId)
                    .Select(si => si.ImageDocument)
                    .ToList();
            }

            // Добавление нового изображения
            public static void AddImage(int studentId, byte[] imageData)
            {
                var newImage = new ImageDocument { ImageDoc = imageData };
                App.context.ImageDocument.Add(newImage);
                App.context.SaveChanges();

                var studentImage = new StudentImage
                {
                    StudentID = studentId,
                    ImageID = newImage.ID
                };
                App.context.StudentImage.Add(studentImage);
                App.context.SaveChanges();
            }

            // Удаление изображения
            public static void DeleteImage(int imageId)
            {
                var image = App.context.ImageDocument.Find(imageId);
                if (image != null)
                {
                    // Удаляем связь со студентом
                    var studentImage = App.context.StudentImage
                        .FirstOrDefault(si => si.ImageID == imageId);
                    if (studentImage != null)
                    {
                        App.context.StudentImage.Remove(studentImage);
                    }

                    App.context.ImageDocument.Remove(image);
                    App.context.SaveChanges();
                }
            }
        }
    }
}
