﻿using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	static class ImageElement
	{

		public static readonly BindableProperty FileImageProperty = BindableProperty.Create("Image", typeof(FileImageSource), typeof(IImageElement), default(FileImageSource),
	propertyChanging: OnImageSourceChanging, propertyChanged: OnImageSourceChanged);

		public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(IImageElement.Source), typeof(ImageSource), typeof(IImageElement), default(ImageSource),
	propertyChanging: OnImageSourceChanging, propertyChanged: OnImageSourceChanged);

		public static readonly BindableProperty AspectProperty = BindableProperty.Create(nameof(IImageElement.Aspect), typeof(Aspect), typeof(IImageElement), Aspect.AspectFit);

		public static readonly BindableProperty IsOpaqueProperty = BindableProperty.Create(nameof(IImageElement.IsOpaque), typeof(bool), typeof(IImageElement), false);


		static void OnImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
		{
			ImageSource newSource = (ImageSource)newValue;
			IImageElement image = (IImageElement)bindable;
			if (newSource != null && image != null)
			{
				newSource.SourceChanged += image.OnImageSourcesSourceChanged;
			}
			ImageSourceChanged(bindable, newSource);
		}

		static void OnImageSourceChanging(BindableObject bindable, object oldValue, object newValue)
		{
			ImageSource oldSource = (ImageSource)oldValue;
			IImageElement image = (IImageElement)bindable;

			if (oldSource != null && image != null)
			{
				oldSource.SourceChanged -= image.OnImageSourcesSourceChanged;
			}
			ImageSourceChanging(oldSource);
		}

		public static SizeRequest Measure(IImageElement ImageElementManager, SizeRequest desiredSize, double widthConstraint, double heightConstraint)
		{
			double desiredAspect = desiredSize.Request.Width / desiredSize.Request.Height;
			double constraintAspect = widthConstraint / heightConstraint;

			double desiredWidth = desiredSize.Request.Width;
			double desiredHeight = desiredSize.Request.Height;

			if (desiredWidth == 0 || desiredHeight == 0)
				return new SizeRequest(new Size(0, 0));

			double width = desiredWidth;
			double height = desiredHeight;
			if (constraintAspect > desiredAspect)
			{
				// constraint area is proportionally wider than image
				switch (ImageElementManager.Aspect)
				{
					case Aspect.AspectFit:
					case Aspect.AspectFill:
						height = Math.Min(desiredHeight, heightConstraint);
						width = desiredWidth * (height / desiredHeight);
						break;
					case Aspect.Fill:
						width = Math.Min(desiredWidth, widthConstraint);
						height = desiredHeight * (width / desiredWidth);
						break;
				}
			}
			else if (constraintAspect < desiredAspect)
			{
				// constraint area is proportionally taller than image
				switch (ImageElementManager.Aspect)
				{
					case Aspect.AspectFit:
					case Aspect.AspectFill:
						width = Math.Min(desiredWidth, widthConstraint);
						height = desiredHeight * (width / desiredWidth);
						break;
					case Aspect.Fill:
						height = Math.Min(desiredHeight, heightConstraint);
						width = desiredWidth * (height / desiredHeight);
						break;
				}
			}
			else
			{
				// constraint area is same aspect as image
				width = Math.Min(desiredWidth, widthConstraint);
				height = desiredHeight * (width / desiredWidth);
			}

			return new SizeRequest(new Size(width, height));
		}

		public static void OnBindingContextChanged(IImageElement image, VisualElement visualElement)
		{
			if (image.Source != null)
				BindableObject.SetInheritedBindingContext(image.Source, visualElement?.BindingContext);
		}


		public static async void ImageSourceChanging(ImageSource oldImageSource)
		{
			if (oldImageSource == null) return;
			try
			{
				await oldImageSource.Cancel().ConfigureAwait(false);
			}
			catch (ObjectDisposedException)
			{
				// Workaround bugzilla 37792 https://bugzilla.xamarin.com/show_bug.cgi?id=37792
			}
		}

		public static void ImageSourceChanged(BindableObject bindable, ImageSource newSource)
		{
			var visualElement = (VisualElement)bindable;
			if (newSource != null)
				BindableObject.SetInheritedBindingContext(newSource, visualElement?.BindingContext);

			visualElement?.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		public static void ImageSourcesSourceChanged(object sender, EventArgs e)
		{
			if (sender is IImageElement imageController)
				imageController.RaiseImageSourcePropertyChanged();

			((VisualElement)sender).InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}
	}
}
