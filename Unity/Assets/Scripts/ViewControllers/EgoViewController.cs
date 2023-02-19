﻿using Assets.Enums;
using Assets.Repos;
using Entity;
using System;
using UnityEngine;

public class EgoViewController : VehicleViewController
{
    public GameObject DestinationPrefab;
    private Ego ego;
    private DestinationController destination;
    private EgoSettingsPopupController egoSettingsController;
    public new void Awake()
    {
        base.Awake();

        this.egoSettingsController = GameObject.Find("PopUps").transform.Find("EgoSettingsPopUp").gameObject.GetComponent<EgoSettingsPopupController>();
        this.egoSettingsController.gameObject.SetActive(true);

        var egoPosition = new Location(transform.position.x, transform.position.y, 0, 0);
        this.ego = new Ego(egoPosition, VehicleModelRepository.getDefaultCarModel(), VehicleCategory.Car, 0); // TODO initial speed: different default later?
        this.ego.setView(this);
    }

    public override Sprite getSprite()
    {
        return Resources.Load<Sprite>("sprites/" + "ego");
    }

    public override void select()
    {
        base.select();
        if(this.destination is null)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var destinationGameObject = Instantiate(DestinationPrefab, new Vector3(mousePosition.x, mousePosition.y, -0.1f), Quaternion.identity);
            this.destination = destinationGameObject.GetComponent<DestinationController>();
            this.destination.setEgo(this);
            this.destination.setColor(this.sprite.color);
        }
        this.destination?.select();
        snapController.IgnoreClicks = true;
    }

    public override void deselect()
    {
        base.deselect();
        destination?.deselect();
        snapController.IgnoreClicks = false;
    }

    public void submitDestination(Location destination)
    {
        ego.Destination = destination;
    }

    public override void destroy()
    {
        this.mainController.setEgo(null);
        destination?.Destroy();
        Destroy(gameObject);
    }

    public override void onChangeColor(Color color)
    {
        if (placed)
        {
            this.sprite.color = color;
        }
        else
        {
            this.sprite.color = new Color(color.r, color.g, color.b, 0.5f);
        }
        this.destination?.setColor(color);
        mainController.refreshEntityList();
    }

    public override void init(VehicleCategory cat)
    {
        ego.setCategory(cat);
        switch (cat)
        {
            case VehicleCategory.Car:
            case VehicleCategory.Motorcycle:
                this.ignoreWaypoints = false;
                return;
            case VehicleCategory.Bike:
            case VehicleCategory.Pedestrian:
                this.ignoreWaypoints = true;
                return;
        }
    }

    public override void onChangeCategory(VehicleCategory cat)
    {
        base.onChangeCategory(cat);
        switch (cat)
        {
            case VehicleCategory.Car:
                sprite.sprite = Resources.Load<Sprite>("sprites/" + "vehicle");
                return;
            case VehicleCategory.Bike:
                sprite.sprite = Resources.Load<Sprite>("sprites/" + "bike");
                return;
            case VehicleCategory.Pedestrian:
                sprite.sprite = Resources.Load<Sprite>("sprites/" + "pedestrian");
                return;
            case VehicleCategory.Motorcycle:
                sprite.sprite = Resources.Load<Sprite>("sprites/" + "motorcycle");
                return;
        }
    }

    public override BaseEntity getEntity()
    {
        return this.ego;
    }

    public override void openEditDialog()
    {
        this.egoSettingsController.open(this, sprite.color);
    }

    protected override void registerEntity()
    {
        mainController.setEgo(this.ego);
    }
}
