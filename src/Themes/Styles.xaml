<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <FontFamily x:Key="AppFont">Segoe UI Variable, Segoe UI</FontFamily>
    <FontFamily x:Key="IconFont">Segoe MDL2 Assets</FontFamily>

    <Style x:Key="HeadingText" TargetType="TextBlock">
        <Setter Property="FontFamily" Value="{StaticResource AppFont}"/>
        <Setter Property="FontSize" Value="26"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Foreground" Value="{DynamicResource PrimaryTextBrush}"/>
    </Style>

    <Style x:Key="SubText" TargetType="TextBlock">
        <Setter Property="FontFamily" Value="{StaticResource AppFont}"/>
        <Setter Property="FontSize" Value="13"/>
        <Setter Property="Foreground" Value="{DynamicResource SecondaryTextBrush}"/>
    </Style>

    <Style x:Key="RoundIconButton" TargetType="Button">
        <Setter Property="Width" Value="40"/>
        <Setter Property="Height" Value="40"/>
        <Setter Property="Background" Value="{DynamicResource SidebarBackgroundBrush}"/>
        <Setter Property="Foreground" Value="{DynamicResource PrimaryTextBrush}"/>
        <Setter Property="FontFamily" Value="{StaticResource IconFont}"/>
        <Setter Property="FontSize" Value="16"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="bd" CornerRadius="20" Background="{TemplateBinding Background}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="bd" Property="Background" Value="{DynamicResource AccentBrush}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key="PillButton" TargetType="Button">
        <Setter Property="Padding" Value="16,8"/>
        <Setter Property="Background" Value="{DynamicResource AccentBrush}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="FontFamily" Value="{StaticResource AppFont}"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="bd" CornerRadius="18" Background="{TemplateBinding Background}"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="bd" Property="Background" Value="{DynamicResource AccentBrushHover}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key="GhostPillButton" TargetType="Button" BasedOn="{StaticResource PillButton}">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Foreground" Value="{DynamicResource PrimaryTextBrush}"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="bd" CornerRadius="18" Background="{TemplateBinding Background}"
                            BorderBrush="{DynamicResource CardBorderBrush}" BorderThickness="1"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="bd" Property="Background" Value="{DynamicResource CardBorderBrush}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <Style x:Key="BillCard" TargetType="Border">
        <Setter Property="Background" Value="{DynamicResource CardBackgroundBrush}"/>
        <Setter Property="BorderBrush" Value="{DynamicResource CardBorderBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="CornerRadius" Value="16"/>
        <Setter Property="Padding" Value="18"/>
        <Setter Property="Margin" Value="0,0,0,12"/>
        <Setter Property="RenderTransformOrigin" Value="0.5,0.5"/>
        <Setter Property="RenderTransform">
            <Setter.Value>
                <ScaleTransform ScaleX="1" ScaleY="1"/>
            </Setter.Value>
        </Setter>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="BorderBrush" Value="{DynamicResource AccentBrush}"/>
            </Trigger>
        </Style.Triggers>
    </Style>

</ResourceDictionary>
