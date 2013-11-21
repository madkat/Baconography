using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class WebViewModel : ContentViewModel
    {
        //preview could look for a large image in the content or it could grab the first paragraph
        //public override void LoadContent()
        //{
            //if nboilerpipe is turned off dont do anything here
            //otherwise load everything
        //}

        //public override void LoadPreview()
        //{
            //useing nboilerpipe grab the article and get either the first paragraph or the first big image near the top
        //}

        protected override Task LoadContent()
        {
            throw new NotImplementedException();
        }
    }
}
